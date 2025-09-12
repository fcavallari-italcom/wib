namespace WIB.Application.Interfaces;

public interface IKieClient
{
    Task<string> ExtractFieldsAsync(string ocrResult, CancellationToken ct);
}
