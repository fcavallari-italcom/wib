from fastapi.testclient import TestClient
from services.ml.main import app

def test_health():
    client = TestClient(app)
    r = client.get("/health")
    assert r.status_code == 200
