import os, sys
sys.path.append(os.path.abspath("."))
import io
from PIL import Image, ImageDraw
from fastapi.testclient import TestClient
import numpy as np

from services.ocr.app.main import app


client = TestClient(app)


def create_image(text: str, fmt: str) -> bytes:
    img = Image.new("RGB", (400, 100), color="white")
    draw = ImageDraw.Draw(img)
    draw.text((10, 30), text, fill="black")
    # add mild noise
    arr = np.array(img)
    noise = np.random.normal(0, 5, arr.shape).astype(np.int16)
    arr = np.clip(arr + noise, 0, 255).astype(np.uint8)
    img = Image.fromarray(arr)
    buf = io.BytesIO()
    img.save(buf, format=fmt)
    return buf.getvalue()


def test_png_ocr():
    data = create_image("Hello 123", "PNG")
    response = client.post(
        "/ocr", files={"file": ("test.png", data, "image/png")}
    )
    assert response.status_code == 200
    payload = response.json()
    assert payload["text"].strip()
    assert isinstance(payload["lines"], list)
    assert "confidenceAvg" in payload


def test_jpg_ocr():
    data = create_image("World 456", "JPEG")
    response = client.post(
        "/ocr", files={"file": ("test.jpg", data, "image/jpeg")}
    )
    assert response.status_code == 200
    payload = response.json()
    assert payload["text"].strip()
    assert isinstance(payload["lines"], list)
    assert "confidenceAvg" in payload
