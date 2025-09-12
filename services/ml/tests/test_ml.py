from fastapi.testclient import TestClient
from services.ml.main import app

client = TestClient(app)

def test_predict():
    r = client.post('/predict', json={'labelRaw': 'milk'})
    assert r.status_code == 200

def test_feedback():
    r = client.post('/feedback', json={'labelRaw': 'milk', 'finalTypeId': '1', 'finalCategoryId': '1'})
    assert r.status_code == 200

def test_train():
    r = client.post('/train')
    assert r.status_code == 200
