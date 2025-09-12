using WIB.Application.Models;

namespace WIB.Application.Interfaces;

public interface IOcrClient
{
    Task<OcrResult> ExtractAsync(Stream image, CancellationToken ct);
}
