using WIB.Application.Interfaces;
using WIB.Application.Models;

namespace WIB.Infrastructure.Clients;

public class KieClient : IKieClient
{
    public Task<ReceiptDraft> ExtractFieldsAsync(OcrResult ocr, CancellationToken ct)
        => Task.FromResult(new ReceiptDraft(null, DateTime.UtcNow, "EUR", Array.Empty<ReceiptLineDraft>(), 0m, null));
}
