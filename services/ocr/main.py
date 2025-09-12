from fastapi import FastAPI, UploadFile, File
from pydantic import BaseModel

class OcrResult(BaseModel):
    text: str
    boxes: list

app = FastAPI()

@app.post("/ocr", response_model=OcrResult)
async def ocr(file: UploadFile = File(...)):
    return OcrResult(text="demo", boxes=[])

@app.get("/health")
def health():
    return {"status": "ok"}
