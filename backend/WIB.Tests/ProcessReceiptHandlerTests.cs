using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WIB.Application.Interfaces;
using WIB.Application.Models;
using WIB.Application.Receipts;
using WIB.Domain;
using Xunit;

namespace WIB.Tests;

public class ProcessReceiptHandlerTests
{
    [Fact]
    public async Task Maps_draft_to_receipt()
    {
        var draft = new ReceiptDraft(
            "Store A",
            new DateTime(2024, 1, 1),
            "EUR",
            new[] { new ReceiptLineDraft("Milk", 2m, 1.5m, 3m, 0.1m, null) },
            3m,
            0.5m);

        var handler = CreateHandler(draft, new Dictionary<string, ClassificationResult>
        {
            ["Milk"] = new ClassificationResult(
                new[] { new ClassificationCandidate(Guid.NewGuid(), 0.9f) },
                Array.Empty<ClassificationCandidate>())
        });

        var receipt = await handler.Handle(new ProcessReceiptCommand(new MemoryStream()), CancellationToken.None);

        Assert.Equal(new DateTime(2024, 1, 1), receipt.Date);
        Assert.Equal("EUR", receipt.Currency);
        Assert.Single(receipt.Lines);
        var line = receipt.Lines.First();
        Assert.Equal("Milk", line.LabelRaw);
        Assert.Equal(2m, line.Qty);
        Assert.Equal(1.5m, line.UnitPrice);
    }

    [Fact]
    public async Task Applies_confidence_threshold()
    {
        var draft = new ReceiptDraft(
            null,
            DateTime.Today,
            "EUR",
            new[]
            {
                new ReceiptLineDraft("Milk", 1m, 1m, 1m, null, null),
                new ReceiptLineDraft("Bread", 1m, 2m, 2m, null, null)
            },
            3m,
            null);

        var high = new ClassificationCandidate(Guid.NewGuid(), 0.9f);
        var low = new ClassificationCandidate(Guid.NewGuid(), 0.6f);

        var handler = CreateHandler(draft, new Dictionary<string, ClassificationResult>
        {
            ["Milk"] = new ClassificationResult(new[] { high }, Array.Empty<ClassificationCandidate>()),
            ["Bread"] = new ClassificationResult(new[] { low }, Array.Empty<ClassificationCandidate>())
        });

        var receipt = await handler.Handle(new ProcessReceiptCommand(new MemoryStream()), CancellationToken.None);

        var milkProduct = receipt.Lines.First(l => l.LabelRaw == "Milk").Product!;
        var breadProduct = receipt.Lines.First(l => l.LabelRaw == "Bread").Product!;
        Assert.Equal(high.Id, milkProduct.ProductTypeId);
        Assert.Equal(Guid.Empty, breadProduct.ProductTypeId);
    }

    [Fact]
    public async Task Persists_price_history()
    {
        var draft = new ReceiptDraft(
            "Store B",
            new DateTime(2024, 2, 2),
            "EUR",
            new[] { new ReceiptLineDraft("Eggs", 1m, 2m, 2m, null, null) },
            2m,
            null);

        var type = new ClassificationCandidate(Guid.NewGuid(), 0.95f);
        var handler = CreateHandler(draft, new Dictionary<string, ClassificationResult>
        {
            ["Eggs"] = new ClassificationResult(new[] { type }, Array.Empty<ClassificationCandidate>())
        });

        var receipt = await handler.Handle(new ProcessReceiptCommand(new MemoryStream()), CancellationToken.None);

        var product = receipt.Lines.Single().Product!;
        var price = product.Prices.Single();
        Assert.Equal(new DateTime(2024, 2, 2), price.Date);
        Assert.Equal(2m, price.UnitPrice);
        Assert.Equal(receipt.StoreId, price.StoreId);
    }

    private static ProcessReceiptCommandHandler CreateHandler(ReceiptDraft draft, Dictionary<string, ClassificationResult> classifications)
    {
        IOcrClient ocr = new FakeOcrClient();
        IKieClient kie = new FakeKieClient(draft);
        IProductClassifier classifier = new FakeClassifier(classifications);
        IReceiptStorage storage = new InMemoryReceiptStorage();
        return new ProcessReceiptCommandHandler(ocr, kie, classifier, storage);
    }

    private class FakeOcrClient : IOcrClient
    {
        public Task<OcrResult> ExtractAsync(Stream image, CancellationToken ct) => Task.FromResult(new OcrResult(""));
    }

    private class FakeKieClient : IKieClient
    {
        private readonly ReceiptDraft _draft;
        public FakeKieClient(ReceiptDraft draft) => _draft = draft;
        public Task<ReceiptDraft> ExtractFieldsAsync(OcrResult ocr, CancellationToken ct) => Task.FromResult(_draft);
    }

    private class FakeClassifier : IProductClassifier
    {
        private readonly IDictionary<string, ClassificationResult> _results;
        public FakeClassifier(IDictionary<string, ClassificationResult> results) => _results = results;
        public Task<ClassificationResult> PredictAsync(string labelRaw, string? brand, CancellationToken ct) => Task.FromResult(_results[labelRaw]);
        public Task FeedbackAsync(string labelRaw, string? brand, Guid finalTypeId, Guid? finalCategoryId, CancellationToken ct) => Task.CompletedTask;
    }

    private class InMemoryReceiptStorage : IReceiptStorage
    {
        public Receipt? Saved { get; private set; }
        public Task<Receipt> SaveAsync(Receipt receipt, CancellationToken ct)
        {
            Saved = receipt;
            return Task.FromResult(receipt);
        }
    }
}
