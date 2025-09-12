from fastapi.testclient import TestClient
from services.ocr.main import app

client = TestClient(app)

def test_health():
    r = client.get('/health')
    assert r.status_code == 200

def test_ocr():
    r = client.post('/ocr', files={'file': ('x.png', b'data')})
    assert r.status_code == 200
    assert 'text' in r.json()
