from fastapi.testclient import TestClient
from services.ocr.main import app


def test_ocr():
    client = TestClient(app)
    files = {"file": ("test.txt", b"hello")}
    r = client.post("/ocr", files=files)
    assert r.status_code == 200
    assert r.json()["text"] == "hello"
