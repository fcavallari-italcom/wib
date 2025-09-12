using WIB.Application.Interfaces;

namespace WIB.Infrastructure.Clients;

public class KieClient : IKieClient
{
    public Task<string> ExtractFieldsAsync(string ocrResult, CancellationToken ct)
        => Task.FromResult("mock-kie");
}
