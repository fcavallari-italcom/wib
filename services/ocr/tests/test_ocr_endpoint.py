import io
import sys
from pathlib import Path
import numpy as np
import cv2
from fastapi.testclient import TestClient

sys.path.append(str(Path(__file__).resolve().parents[1]))
from app.main import app  # type: ignore
import app.main as main  # type: ignore


def test_ocr_endpoint(monkeypatch):
    main.paddle_ocr = None

    def fake_ocr(image):
        return "hello", [{"text": "hello", "box": [[0, 0], [1, 1]]}], 0.9

    monkeypatch.setattr(main, "ocr_with_tesseract", fake_ocr)

    img = np.zeros((10, 10, 3), dtype=np.uint8)
    _, buffer = cv2.imencode(".png", img)
    client = TestClient(app)
    files = {"file": ("test.png", io.BytesIO(buffer.tobytes()), "image/png")}
    res = client.post("/ocr", files=files)
    assert res.status_code == 200
    data = res.json()
    assert data["text"] == "hello"
    assert data["lines"][0]["text"] == "hello"
