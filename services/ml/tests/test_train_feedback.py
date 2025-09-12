import json
import os
import tempfile
import importlib
from pathlib import Path
import sys

from fastapi.testclient import TestClient

sys.path.append(str(Path(__file__).resolve().parents[3]))


def load_app(dataset):
    models_dir = tempfile.mkdtemp()
    os.environ["MODELS_DIR"] = models_dir
    train_path = Path(models_dir) / "train.jsonl"
    with open(train_path, "w") as f:
        for row in dataset:
            f.write(json.dumps(row) + "\n")
    os.environ["TRAIN_DATA_PATH"] = str(train_path)
    import services.ml.main as ml
    importlib.reload(ml)
    return ml.app, ml.MODELS_DIR, ml.OnlineTextClassifier


def _precision(client, samples, key):
    correct = 0
    for s in samples:
        r = client.post("/predict", json={"labelRaw": s["labelRaw"]})
        cand = r.json().get(f"{key}Candidates", [])
        if cand and cand[0]["id"] == s[f"{key}_id"]:
            correct += 1
    return correct / len(samples)


def test_precision_improves_after_training():
    dataset = [
        {"labelRaw": "milk", "type_id": "drink", "type_name": "Drink", "category_id": "dairy", "category_name": "Dairy"},
        {"labelRaw": "bread", "type_id": "food", "type_name": "Food", "category_id": "bakery", "category_name": "Bakery"},
    ]
    app, _, _ = load_app(dataset)
    client = TestClient(app)
    baseline_type = _precision(client, dataset, "type")
    baseline_cat = _precision(client, dataset, "category")
    resp = client.post("/train")
    assert resp.json().get("status") == "ok"
    trained_type = _precision(client, dataset, "type")
    trained_cat = _precision(client, dataset, "category")
    assert trained_type > baseline_type
    assert trained_cat > baseline_cat


def test_models_saved_and_loaded():
    dataset = [
        {"labelRaw": "milk", "type_id": "drink", "type_name": "Drink", "category_id": "dairy", "category_name": "Dairy"},
        {"labelRaw": "bread", "type_id": "food", "type_name": "Food", "category_id": "bakery", "category_name": "Bakery"},
    ]
    app, models_path, OnlineTextClassifier = load_app(dataset)
    client = TestClient(app)
    resp = client.post("/train")
    assert resp.json().get("status") == "ok"
    type_path = Path(models_path) / "type.joblib"
    assert type_path.exists()
    loaded = OnlineTextClassifier.load(type_path)
    probs = loaded.predict_proba("milk")
    assert "drink" in probs


def test_feedback_updates_prediction():
    dataset = [
        {"labelRaw": "milk", "type_id": "drink", "type_name": "Drink", "category_id": "dairy", "category_name": "Dairy"},
        {"labelRaw": "bread", "type_id": "food", "type_name": "Food", "category_id": "bakery", "category_name": "Bakery"},
    ]
    app, _, _ = load_app(dataset)
    client = TestClient(app)
    client.post("/train")
    r1 = client.post("/predict", json={"labelRaw": "hammer"})
    before = next((c["conf"] for c in r1.json()["typeCandidates"] if c["id"] == "drink"), 0)
    client.post(
        "/feedback",
        json={"labelRaw": "hammer", "finalTypeId": "drink", "finalCategoryId": "dairy"},
    )
    r2 = client.post("/predict", json={"labelRaw": "hammer"})
    after = next((c["conf"] for c in r2.json()["typeCandidates"] if c["id"] == "drink"), 0)
    assert after > before
