from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
import numpy as np
import cv2
from PIL import Image
import io
import re

app = FastAPI(title="OCR Service")

# Attempt to load PaddleOCR, fall back to pytesseract
paddle_ocr = None
try:
    from paddleocr import PaddleOCR  # type: ignore

    paddle_ocr = PaddleOCR(lang="it", use_angle_cls=True)
except Exception:  # pragma: no cover - paddle is optional
    paddle_ocr = None

try:
    import pytesseract  # type: ignore
except Exception as e:  # pragma: no cover - should not happen if installed
    raise e


def deskew(image: np.ndarray) -> np.ndarray:
    """Basic deskew placeholder; returns image unchanged."""
    return image


def denoise(image: np.ndarray) -> np.ndarray:
    return cv2.fastNlMeansDenoisingColored(image, None, 10, 10, 7, 21)


def extract_kie(text: str, lines: list):
    date_match = re.search(r"\b\d{1,2}/\d{1,2}/\d{2,4}\b", text)
    total_match = re.search(r"\b\d+[,.]\d{2}\b", text)
    store = lines[0]["text"] if lines else None
    return {
        "date": date_match.group(0) if date_match else None,
        "total": total_match.group(0) if total_match else None,
        "store": store,
    }


def ocr_with_paddle(image: np.ndarray):
    result = paddle_ocr.ocr(image, cls=True)  # type: ignore
    lines = []
    confidences = []
    texts = []
    for line in result:
        for box, (txt, conf) in line:
            lines.append({"text": txt, "box": box})
            confidences.append(conf)
            texts.append(txt)
    full_text = " ".join(texts)
    confidence = float(np.mean(confidences)) if confidences else 0.0
    return full_text, lines, confidence


def ocr_with_tesseract(image: np.ndarray):
    pil_img = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
    data = None
    for lang in ["ita", "eng"]:
        try:
            data = pytesseract.image_to_data(
                pil_img, lang=lang, output_type=pytesseract.Output.DICT
            )
            break
        except pytesseract.TesseractError:
            continue
    if data is None:
        raise RuntimeError("Tesseract OCR failed")
    n = len(data["text"])
    lines = []
    texts = []
    confidences = []
    for i in range(n):
        txt = data["text"][i].strip()
        conf = float(data["conf"][i])
        if txt and conf > 0:
            texts.append(txt)
            confidences.append(conf)
            box = [
                [data["left"][i], data["top"][i]],
                [data["left"][i] + data["width"][i], data["top"][i] + data["height"][i]],
            ]
            lines.append({"text": txt, "box": box})
    full_text = " ".join(texts)
    confidence = float(np.mean(confidences)) if confidences else 0.0
    return full_text, lines, confidence


@app.post("/ocr")
async def ocr_endpoint(file: UploadFile = File(...)):
    if file.content_type not in ("image/png", "image/jpeg"):
        raise HTTPException(status_code=400, detail="Unsupported file type")
    contents = await file.read()
    image = Image.open(io.BytesIO(contents))
    image = cv2.cvtColor(np.array(image), cv2.COLOR_RGB2BGR)
    image = denoise(deskew(image))

    if paddle_ocr:
        text, lines, confidence = ocr_with_paddle(image)
    else:
        text, lines, confidence = ocr_with_tesseract(image)

    response = {
        "text": text,
        "lines": lines,
        "blocks": [],
        "confidenceAvg": confidence,
        "kie": extract_kie(text, lines),
    }
    return JSONResponse(response)
