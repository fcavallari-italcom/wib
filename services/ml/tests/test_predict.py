from fastapi.testclient import TestClient
from services.ml.main import app

def test_predict():
    client = TestClient(app)
    r = client.post("/predict", json={"labelRaw": "milk"})
    assert r.status_code == 200
    assert r.json() == {"typeCandidates": [], "categoryCandidates": []}
