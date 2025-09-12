from fastapi import FastAPI, UploadFile, File
from pydantic import BaseModel

app = FastAPI()

class OcrResponse(BaseModel):
    text: str
    boxes: list[list[int]] = []

@app.post("/ocr", response_model=OcrResponse)
async def ocr(file: UploadFile = File(...)):
    content = await file.read()
    return OcrResponse(text=content.decode(errors="ignore"), boxes=[])

@app.get("/health")
def health():
    return {"status": "ok"}
