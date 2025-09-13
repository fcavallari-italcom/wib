using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WIB.Application.Models;
using WIB.Infrastructure.Clients;
using Xunit;

namespace WIB.Tests;

public class KieClientTests
{
    [Fact]
    public async Task ExtractFieldsAsync_matches_snapshot()
    {
        var client = new KieClient();
        var result = await client.ExtractFieldsAsync(new OcrResult("sample"), CancellationToken.None);
        result = result with { Date = new DateTime(2000, 1, 1) };
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        var snapshot = await File.ReadAllTextAsync(Path.Combine("Snapshots", "ReceiptDraft.json"));
        Assert.Equal(snapshot.Trim(), json.Trim());
    }
}
