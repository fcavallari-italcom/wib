using WIB.Application.Interfaces;
using WIB.Application.Models;

namespace WIB.Infrastructure.Clients;

public class OcrClient : IOcrClient
{
    public Task<OcrResult> ExtractAsync(Stream image, CancellationToken ct)
        => Task.FromResult(new OcrResult("mock-ocr"));
}
