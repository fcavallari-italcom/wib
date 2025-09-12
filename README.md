# Where I Buy Monorepo

## Avvio rapido

- `dotnet build backend/WIB.sln`
- `python -m pytest services/ocr/tests services/ml/tests`
- `npm run check:angular`
- `npm run lint --prefix frontend`
- `docker compose up`

## Obiettivi

- Caricare foto/screenshot di **scontrini** da app Angular (mobile-first, fotocamera del telefono).
- **OCR + parsing** totalmente **on-prem / offline** (nessuna dipendenza da API esterne).
- **Normalizzazione** prodotti, **categoria** e **tipo commerciale (nome merceologico)**.
- **Tracking prezzi** per supermercato e **budgeting**/uscite mensili.
- **Stack**: Backend **.NET 8** (Web API + worker), Frontend **Angular 19** (no SSR), DB **PostgreSQL**, storage **MinIO**, cache **Redis**.
- **AI locale**: OCR (PaddleOCR/Tesseract) + **KIE** (Key Information Extraction) e **classificazione prodotti** con modelli open-source (Hugging Face) eseguiti in un microservizio Python.

## Architettura logica

```
[Angular 19 (PWA mobile)] → [WIB.API (.NET 8)] → [Queue] → [WIB.Worker]
      │                         │                   │
      │                         ├──> [MinIO: immagini]
      │                         ├──> [PostgreSQL]
      │                         └──> [Redis]
      │
      └── Upload foto scontrino, dashboard, confronto prezzi

[WIB.Worker]:
  1) scarica immagine da MinIO
  2) OCR locale (PaddleOCR o Tesseract) + KIE (layout/chiavi)
  3) Parsing righe articoli
  4) Normalizzazione + Matching → Classificatore locale (categoria + tipo commerciale)
  5) Persistenza (Receipt/Lines, Product, PriceHistory)

[WIB.ML] (microservizio Python):
  - Endpoint /predict per suggerire Categoria e Tipo commerciale
  - Endpoint /feedback per apprendere dai correzioni manuali (online learning)
  - Batch /train per riaddestramento periodico
```

## Modello dati (relazionale)

- **Store**(Id, Name, Chain, Address, City)
- **ProductType**(Id, Name, Aliases JSONB) ← *tipo commerciale / nome merceologico*
- **Category**(Id, Name, ParentId FK nullable)
- **Product**(Id, Name, Brand, GTIN nullable, **ProductTypeId FK**, **CategoryId FK nullable**)
- **ProductAlias**(Id, ProductId FK, Alias)
- **Receipt**(Id, StoreId FK, Date, Total, TaxTotal nullable, Currency, RawText)
- **ReceiptLine**(Id, ReceiptId FK, ProductId FK nullable, LabelRaw, Qty decimal(10,3), UnitPrice decimal(10,3), LineTotal decimal(10,3), VatRate nullable)
- **PriceHistory**(Id, ProductId FK, StoreId FK, Date, UnitPrice)
- **BudgetMonth**(Id, Year, Month, LimitAmount)
- **ExpenseAggregate**(Id, Year, Month, StoreId nullable, CategoryId nullable, Amount)
- **LabelingEvent**(Id, ProductId FK nullable, LabelRaw, PredictedTypeId nullable, PredictedCategoryId nullable, FinalTypeId, FinalCategoryId, Confidence decimal(3,2), WhenUtc)

> Nota: `ProductType` è *cross-brand* (es.: **Farina 0**, **Farina Tipo 1**, **Farina Manitoba**). Prodotti diversi (Caputo Nuvola, Mulino Rossetto 0) condividono lo stesso `ProductType` ma possono avere `Category` diverse in gerarchie più ampie (Es. Alimenti > Dispensa > Farine).

## docker-compose.yml (locale, senza OpenAI)

```yaml
services:
  api:
    build: ./backend
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__Default=Host=db;Database=wib;Username=wib;Password=wib
      - Minio__Endpoint=minio:9000
      - Minio__AccessKey=wib
      - Minio__SecretKey=wibsecret
      - Redis__Connection=redis:6379
      - Ocr__Endpoint=http://ocr:8081
      - Ml__Endpoint=http://ml:8082
    ports: ["8080:8080"]
    depends_on: [db, minio, redis, ocr, ml]

  worker:
    build: ./worker
    environment:
      - ConnectionStrings__Default=Host=db;Database=wib;Username=wib;Password=wib
      - Minio__Endpoint=minio:9000
      - Minio__AccessKey=wib
      - Minio__SecretKey=wibsecret
      - Redis__Connection=redis:6379
      - Ocr__Endpoint=http://ocr:8081
      - Ml__Endpoint=http://ml:8082
    depends_on: [db, minio, redis, ocr, ml]

  db:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=wib
      - POSTGRES_USER=wib
      - POSTGRES_PASSWORD=wib
    volumes: ["./.data/db:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine

  minio:
    image: minio/minio:latest
    environment:
      - MINIO_ROOT_USER=wib
      - MINIO_ROOT_PASSWORD=wibsecret
    command: server /data --console-address ":9001"
    ports: ["9000:9000", "9001:9001"]
    volumes: ["./.data/minio:/data"]

  # OCR server locale (FastAPI + PaddleOCR CPU)
  ocr:
    build: ./services/ocr
    ports: ["8081:8081"]

  # Classificatore Categoria & Tipo commerciale (FastAPI + scikit-learn + embeddings locale)
  ml:
    build: ./services/ml
    ports: ["8082:8082"]
    volumes:
      - ./.data/models:/app/models

  # (opzionale ma consigliato) vector DB per KNN semantico
  qdrant:
    image: qdrant/qdrant:latest
    ports: ["6333:6333"]
    volumes: ["./.data/qdrant:/qdrant/storage"]
```

> OCR: container custom per evitare dipendenze non stabili. Alternativa: Tesseract-only se l’hardware è molto limitato.

## Backend (.NET 8) – Progetto WIB.API

**Punti chiave (SOLID):**

- `IReceiptStorage`, `IOcrClient`, `IKieClient` (Key-Info Extraction), `IProductMatcher`, `IProductClassifier` → inversione delle dipendenze.
- `ReceiptController` minimale; orchestrazione in `ProcessReceiptCommandHandler` (CQRS-style).
- Persistence con **EF Core 8**; migrations.

**Struttura cartelle**

```
backend/
  WIB.API/
  WIB.Domain/
  WIB.Infrastructure/
  WIB.Application/
  WIB.Worker/
```

**Interfacce client**

```csharp
public interface IOcrClient { Task<OcrResult> ExtractAsync(Stream image, CancellationToken ct); }
public interface IKieClient { Task<ReceiptDraft> ExtractFieldsAsync(OcrResult ocr, CancellationToken ct); }
public interface IProductClassifier { Task<ClassificationResult> PredictAsync(string labelRaw, CancellationToken ct); Task FeedbackAsync(string labelRaw, string? brand, Guid typeId, Guid? categoryId, CancellationToken ct); }
```

**DTO target (on-prem)**

```json
{
  "store": {"name": "string", "address": "string", "chain": "string"},
  "datetime": "ISO-8601",
  "currency": "EUR",
  "lines": [
    {"labelRaw": "string", "qty": 1, "unitPrice": 1.23, "lineTotal": 1.23, "vatRate": 4}
  ],
  "totals": {"subtotal": 0, "tax": 0, "total": 0}
}
```

**Worker (outline)**

```csharp
// - download immagine da MinIO
// - IOcrClient.ExtractAsync -> testo + bounding boxes
// - IKieClient.ExtractFieldsAsync -> campi chiave + righe
// - Per ogni riga: IProductClassifier.PredictAsync(LabelRaw) -> (TypeId?, CategoryId?, confidence)
// - Applica soglia; se bassa, marca come "da confermare" e accoda LabelingEvent
// - Persisti tutto e aggiorna PriceHistory
```

## Estrazione locale (niente cloud)

- **OCR**: PaddleOCR CPU (italiano + numeri), con pre-processing (deskew, denoise) in Worker (ImageSharp/Magick.NET).
- **KIE (Key-Info Extraction)** su scontrini:
  - Opzione A: **PaddleOCR PP-Structure (SER/RE)** per campi Store/Date/Totale e delimitazione righe.
  - Opzione B: **Donut (NAVER)** fine-tunato su dataset di scontrini propri (end-to-end, OCR-free). Richiede qualche decina/centinaio di esempi.
- **Parsing righe**: fallback robusto regex/tokenizer quando il layout non è tabellare.

## Matching & normalizzazione prodotto

- **Heuristics**: lower-case, rimozione stopwords di scontrino ("TOTALE", "REPARTO"), pulizia SKU interni.
- **Fuzzy**: distanza token/Levenshtein per aggancio a `ProductAlias` esistenti.
- **Vector** *(opzionale ma consigliato)*: embedding locale (es. `sentence-transformers/all-MiniLM-L6-v2`) + Qdrant per KNN di candidati.

## Classificazione predittiva (Categoria & Tipo commerciale)

**Requisiti**

- Apprendimento **incrementale** dai dati etichettati a mano.
- Suggerimenti top-k con **confidence** e soglia di "rifiuto" → invio a revisione.

**Approccio ibrido (tutto locale)**

1) **Feature testuali** da `labelRaw` + (opz.) brand estratto:
   - TF‑IDF (char 3–5 + word 1–2) → input per `SGDClassifier`/`LinearSVM` con `partial_fit`.
   - **Embedding semantico** (MiniLM/E5) → KNN (Qdrant) per nearest neighbors.
2) **Ensemble**: media pesata di (Linear + KNN) per predire **ProductType** e **Category** separatamente.
3) **Online learning**: ogni conferma/correzione dell’utente → `FeedbackAsync` salva esempio e aggiorna il modello (e/o programma un retrain batch notturno).

**API del microservizio ML (FastAPI)**

- `POST /predict { labelRaw, brand? } → { typeCandidates:[{id,name,conf}], categoryCandidates:[...] }`
- `POST /feedback { labelRaw, brand?, finalTypeId, finalCategoryId }`
- `POST /train` (batch) salva modelli in `/app/models`.

**Persistenza ML**

- Modelli `*.joblib`, vocabolari TF‑IDF, e vettori nel volume `./.data/models`.

**UI assistita (human-in-the-loop)**

- Nel dettaglio scontrino: per ogni riga non certa, chip suggerimenti **Tipo**/**Categoria** con scorri‑to‑apply.
- Schermata “Coda di etichettazione” per validare rapidamente esempi nuovi (shortcut da tastiera, bulk apply).

## Frontend (Angular 19)

- **App DEVICES (mobile PWA)** per scatto e upload: `input type="file" accept="image/*" capture="environment"`, anteprima, compressione client (max 2048px lato lungo) e metadata (store opzionale via geoloc/QR).
- **App WMC (desktop)** per report, confronto prezzi, revisione etichette.
- **Config runtime** via `window.__env` o JSON remoto (niente SSR).

**Feature**

- Upload con stato (SignalR/polling), coda di processazione, notifiche "pronto da verificare".
- Dashboard: spesa mensile, top categorie, andamento prezzi per prodotto & supermercato.
- Confronto prezzi: miglior prezzo recente per **Tipo commerciale** nelle catene frequentate.

## API chiave

- `POST /receipts` (upload)
- `GET /receipts/{id}`
- `GET /analytics/spending?from=...&to=...`
- `GET /analytics/price-history?productId=...&storeId=...`
- `POST /products/{id}/aliases`
- `GET /ml/suggestions?labelRaw=...` (proxy verso ML)
- `POST /ml/feedback` (proxy verso ML)
