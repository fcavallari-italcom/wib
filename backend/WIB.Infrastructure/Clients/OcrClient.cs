using WIB.Application.Interfaces;

namespace WIB.Infrastructure.Clients;

public class OcrClient : IOcrClient
{
    public Task<string> ExtractAsync(Stream image, CancellationToken ct)
        => Task.FromResult("mock-ocr");
}
