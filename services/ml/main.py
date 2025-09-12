from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional

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

app = FastAPI()

@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest):
    return PredictResponse()

@app.post("/feedback")
def feedback(req: FeedbackRequest):
    return {"status": "ok"}

@app.post("/train")
def train():
    return {"status": "ok"}

@app.get("/health")
def health():
    return {"status": "ok"}
