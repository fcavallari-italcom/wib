import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parents[3]))

from fastapi.testclient import TestClient
from services.ml.main import app

def test_health():
    client = TestClient(app)
    r = client.get("/health")
    assert r.status_code == 200
