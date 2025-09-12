using WIB.Application.Models;

namespace WIB.Application.Interfaces;

public interface IKieClient
{
    Task<ReceiptDraft> ExtractFieldsAsync(OcrResult ocr, CancellationToken ct);
}
