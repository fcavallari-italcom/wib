using WIB.Application.Interfaces;
using WIB.Application.Models;
using WIB.Domain;

namespace WIB.Application.Receipts;

public class ProcessReceiptCommandHandler
{
    private const float ConfidenceThreshold = 0.8f;

    private readonly IOcrClient _ocr;
    private readonly IKieClient _kie;
    private readonly IProductClassifier _classifier;
    private readonly IReceiptStorage _storage;

    public ProcessReceiptCommandHandler(IOcrClient ocr, IKieClient kie, IProductClassifier classifier, IReceiptStorage storage)
    {
        _ocr = ocr;
        _kie = kie;
        _classifier = classifier;
        _storage = storage;
    }

    public async Task<Receipt> Handle(ProcessReceiptCommand command, CancellationToken ct)
    {
        var ocrResult = await _ocr.ExtractAsync(command.Image, ct);
        var draft = await _kie.ExtractFieldsAsync(ocrResult, ct);

        var store = new Store { Name = draft.Store ?? string.Empty };
        var receipt = new Receipt
        {
            Store = store,
            StoreId = store.Id,
            Date = draft.Date,
            Currency = draft.Currency,
            Total = draft.Total,
            TaxTotal = draft.TaxTotal
        };

        foreach (var line in draft.Lines)
        {
            var classification = await _classifier.PredictAsync(line.Label, line.Brand, ct);
            var typeCandidate = classification.TypeCandidates?.OrderByDescending(c => c.Confidence).FirstOrDefault();
            var categoryCandidate = classification.CategoryCandidates?.OrderByDescending(c => c.Confidence).FirstOrDefault();

            var product = new Product
            {
                Name = line.Label,
                Brand = line.Brand
            };

            if (typeCandidate != null && typeCandidate.Confidence >= ConfidenceThreshold)
            {
                product.ProductTypeId = typeCandidate.Id;
            }

            if (categoryCandidate != null && categoryCandidate.Confidence >= ConfidenceThreshold)
            {
                product.CategoryId = categoryCandidate.Id;
            }

            product.Prices.Add(new PriceHistory
            {
                Store = store,
                StoreId = store.Id,
                Date = draft.Date,
                UnitPrice = line.UnitPrice
            });

            var receiptLine = new ReceiptLine
            {
                LabelRaw = line.Label,
                Qty = line.Qty,
                UnitPrice = line.UnitPrice,
                LineTotal = line.LineTotal,
                VatRate = line.VatRate,
                Product = product,
                ProductId = product.Id
            };

            receipt.Lines.Add(receiptLine);

            var ev = new LabelingEvent
            {
                Product = product,
                ProductId = product.Id,
                LabelRaw = line.Label,
                PredictedTypeId = typeCandidate?.Id,
                PredictedCategoryId = categoryCandidate?.Id,
                FinalTypeId = product.ProductTypeId,
                FinalCategoryId = product.CategoryId ?? Guid.Empty,
                Confidence = (decimal)Math.Min(typeCandidate?.Confidence ?? 0f, categoryCandidate?.Confidence ?? 0f),
                WhenUtc = DateTime.UtcNow
            };
            // LabelingEvent is created but persistence depends on infrastructure
        }

        await _storage.SaveAsync(receipt, ct);
        return receipt;
    }
}
