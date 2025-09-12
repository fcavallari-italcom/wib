import os
import json
import time
from pathlib import Path
from typing import Dict, List, Optional

from fastapi import FastAPI
from pydantic import BaseModel
from scipy.sparse import hstack
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import SGDClassifier
import joblib
import numpy as np
from sentence_transformers import SentenceTransformer
from qdrant_client import QdrantClient, models


# ----------------------------------------------------------------------------
# Utility classes
# ----------------------------------------------------------------------------

class OnlineTextClassifier:
    """TF-IDF + SGD classifier with online learning support."""

    def __init__(self):
        self.char_vectorizer = TfidfVectorizer(analyzer="char", ngram_range=(3, 5))
        self.word_vectorizer = TfidfVectorizer(analyzer="word", ngram_range=(1, 2))
        self.clf = SGDClassifier(loss="log_loss", random_state=0)
        self.classes_: List[str] = []
        self.id_to_name: Dict[str, str] = {}
        self.fitted = False

    # ------------------------------------------------------------------
    def _transform(self, texts: List[str]):
        x_char = self.char_vectorizer.transform(texts)
        x_word = self.word_vectorizer.transform(texts)
        return hstack([x_char, x_word])

    def fit(self, texts: List[str], labels: List[str]):
        x_char = self.char_vectorizer.fit_transform(texts)
        x_word = self.word_vectorizer.fit_transform(texts)
        x = hstack([x_char, x_word])
        self.classes_ = sorted(set(labels))
        self.clf.partial_fit(x, labels, classes=self.classes_)
        self.fitted = True

    def partial_fit(self, texts: List[str], labels: List[str]):
        if not self.fitted:
            self.fit(texts, labels)
            return
        x = self._transform(texts)
        new_classes = sorted(set(labels) | set(self.classes_))
        self.clf.partial_fit(x, labels, classes=new_classes)
        self.classes_ = new_classes
        self.fitted = True

    def predict_proba(self, text: str) -> Dict[str, float]:
        if not self.fitted:
            return {}
        x = self._transform([text])
        probs = self.clf.predict_proba(x)[0]
        return {cls: float(prob) for cls, prob in zip(self.clf.classes_, probs)}

    # ------------------------------------------------------------------
    def save(self, path: Path):
        data = {
            "char_vectorizer": self.char_vectorizer,
            "word_vectorizer": self.word_vectorizer,
            "clf": self.clf,
            "classes": self.classes_,
            "id_to_name": self.id_to_name,
        }
        joblib.dump(data, path)

    @classmethod
    def load(cls, path: Path) -> "OnlineTextClassifier":
        data = joblib.load(path)
        obj = cls()
        obj.char_vectorizer = data["char_vectorizer"]
        obj.word_vectorizer = data["word_vectorizer"]
        obj.clf = data["clf"]
        obj.classes_ = data.get("classes", [])
        obj.id_to_name = data.get("id_to_name", {})
        obj.fitted = True if obj.classes_ else False
        return obj


class EmbeddingIndex:
    """Wrapper around an in-memory Qdrant collection."""

    def __init__(self, dim: int):
        self.client = QdrantClient(location=":memory:")
        self.collection_name = "ml_embeddings"
        self.dim = dim
        self._next_id = 0
        self._recreate()

    def _recreate(self):
        self.client.recreate_collection(
            collection_name=self.collection_name,
            vectors_config=models.VectorParams(size=self.dim, distance=models.Distance.COSINE),
        )

    def clear(self):
        self._next_id = 0
        self._recreate()

    def add(self, vector: List[float], payload: Dict):
        point = models.PointStruct(id=self._next_id, vector=vector, payload=payload)
        self.client.upsert(collection_name=self.collection_name, points=[point])
        self._next_id += 1

    def search(self, vector: List[float], k: int = 5):
        return self.client.search(collection_name=self.collection_name, query_vector=vector, limit=k)


# ----------------------------------------------------------------------------
# Initialisation
# ----------------------------------------------------------------------------

MODELS_DIR = Path(os.getenv("MODELS_DIR", Path(__file__).resolve().parent / "models"))
MODELS_DIR.mkdir(parents=True, exist_ok=True)

TRAIN_DATA_PATH = os.getenv("TRAIN_DATA_PATH")

EMBED_MODEL_NAME = os.getenv("EMBED_MODEL", "sentence-transformers/all-MiniLM-L6-v2")
try:
    embedder = SentenceTransformer(EMBED_MODEL_NAME)
    EMBED_DIM = embedder.get_sentence_embedding_dimension()
except Exception:
    # fallback to zero embeddings when model cannot be loaded
    EMBED_DIM = 384

    class DummyEmbedder:
        def get_sentence_embedding_dimension(self):
            return EMBED_DIM

        def encode(self, texts):
            return np.zeros((len(texts), EMBED_DIM))

    embedder = DummyEmbedder()

embedding_index = EmbeddingIndex(embedder.get_sentence_embedding_dimension())

TYPE_MODEL_PATH = MODELS_DIR / "type.joblib"
CATEGORY_MODEL_PATH = MODELS_DIR / "category.joblib"

if TYPE_MODEL_PATH.exists():
    type_model = OnlineTextClassifier.load(TYPE_MODEL_PATH)
else:
    type_model = OnlineTextClassifier()

if CATEGORY_MODEL_PATH.exists():
    category_model = OnlineTextClassifier.load(CATEGORY_MODEL_PATH)
else:
    category_model = OnlineTextClassifier()

# ----------------------------------------------------------------------------
# API schemas
# ----------------------------------------------------------------------------

class PredictRequest(BaseModel):
    labelRaw: str
    brand: Optional[str] = None

class Candidate(BaseModel):
    id: str
    name: str
    conf: float

class PredictResponse(BaseModel):
    typeCandidates: List[Candidate] = []
    categoryCandidates: List[Candidate] = []

class FeedbackRequest(BaseModel):
    labelRaw: str
    brand: Optional[str] = None
    finalTypeId: str
    finalCategoryId: Optional[str] = None

# ----------------------------------------------------------------------------
# FastAPI application
# ----------------------------------------------------------------------------

app = FastAPI()


# Helper to combine tfidf and embedding scores
TFIDF_WEIGHT = 0.7
EMBED_WEIGHT = 0.3


def _combine_scores(tfidf_scores: Dict[str, float], emb_scores: Dict[str, float]):
    labels = set(tfidf_scores) | set(emb_scores)
    combined = {}
    for lbl in labels:
        combined[lbl] = TFIDF_WEIGHT * tfidf_scores.get(lbl, 0.0) + EMBED_WEIGHT * emb_scores.get(lbl, 0.0)
    return combined


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest, top_k: int = 3, threshold: float = 0.0):
    text = req.labelRaw

    type_probs = type_model.predict_proba(text)
    cat_probs = category_model.predict_proba(text)

    emb = embedder.encode([text])[0].tolist()
    neighbors = embedding_index.search(emb, k=5)

    type_emb_scores: Dict[str, float] = {}
    cat_emb_scores: Dict[str, float] = {}
    for hit in neighbors:
        score = max(hit.score, 0.0)
        payload = hit.payload or {}
        t = payload.get("type_id")
        c = payload.get("category_id")
        if t:
            type_emb_scores[t] = type_emb_scores.get(t, 0.0) + score
        if c:
            cat_emb_scores[c] = cat_emb_scores.get(c, 0.0) + score

    # normalise embedding scores
    if type_emb_scores:
        max_s = max(type_emb_scores.values()) or 1.0
        type_emb_scores = {k: v / max_s for k, v in type_emb_scores.items()}
    if cat_emb_scores:
        max_s = max(cat_emb_scores.values()) or 1.0
        cat_emb_scores = {k: v / max_s for k, v in cat_emb_scores.items()}

    type_scores = _combine_scores(type_probs, type_emb_scores)
    cat_scores = _combine_scores(cat_probs, cat_emb_scores)

    type_candidates = [
        Candidate(id=lbl, name=type_model.id_to_name.get(lbl, lbl), conf=score)
        for lbl, score in sorted(type_scores.items(), key=lambda kv: kv[1], reverse=True)
        if score >= threshold
    ][:top_k]

    category_candidates = [
        Candidate(id=lbl, name=category_model.id_to_name.get(lbl, lbl), conf=score)
        for lbl, score in sorted(cat_scores.items(), key=lambda kv: kv[1], reverse=True)
        if score >= threshold
    ][:top_k]

    return PredictResponse(typeCandidates=type_candidates, categoryCandidates=category_candidates)


@app.post("/feedback")
def feedback(req: FeedbackRequest):
    text = req.labelRaw
    type_id = req.finalTypeId
    cat_id = req.finalCategoryId

    # append feedback to file
    with open(MODELS_DIR / "feedback.jsonl", "a") as f:
        f.write(json.dumps(req.dict()) + "\n")

    # update models
    type_model.id_to_name.setdefault(type_id, type_id)
    type_model.partial_fit([text], [type_id])
    type_model.save(TYPE_MODEL_PATH)

    if cat_id:
        category_model.id_to_name.setdefault(cat_id, cat_id)
        category_model.partial_fit([text], [cat_id])
        category_model.save(CATEGORY_MODEL_PATH)

    # update embedding index
    emb = embedder.encode([text])[0].tolist()
    payload = {"type_id": type_id}
    if cat_id:
        payload["category_id"] = cat_id
    embedding_index.add(emb, payload)

    # schedule retrain
    with open(MODELS_DIR / "retrain.todo", "w") as f:
        f.write(str(time.time()))

    return {"status": "ok"}


@app.post("/train")
def train(query: str = ""):
    global embedding_index
    train_path = Path(os.getenv("TRAIN_DATA_PATH", TRAIN_DATA_PATH or ""))
    if not train_path.exists():
        return {"status": "no-data"}

    dataset = [json.loads(line) for line in open(train_path)]
    if not dataset:
        return {"status": "no-data"}

    texts = [d["labelRaw"] for d in dataset]
    type_labels = [d["type_id"] for d in dataset]
    cat_labels = [d["category_id"] for d in dataset]

    type_model.fit(texts, type_labels)
    type_model.id_to_name = {d["type_id"]: d.get("type_name", d["type_id"]) for d in dataset}
    type_model.save(TYPE_MODEL_PATH)

    category_model.fit(texts, cat_labels)
    category_model.id_to_name = {d["category_id"]: d.get("category_name", d["category_id"]) for d in dataset}
    category_model.save(CATEGORY_MODEL_PATH)

    # rebuild embeddings index
    embedding_index = EmbeddingIndex(embedder.get_sentence_embedding_dimension())
    for d in dataset:
        emb = embedder.encode([d["labelRaw"]])[0].tolist()
        embedding_index.add(emb, {"type_id": d["type_id"], "category_id": d["category_id"]})

    return {"status": "ok"}


@app.get("/health")
def health():
    return {"status": "ok"}
