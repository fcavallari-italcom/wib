using WIB.Application.Interfaces;

namespace WIB.Infrastructure.Clients;

public class ProductClassifier : IProductClassifier
{
    public Task<(Guid? TypeId, Guid? CategoryId, float Confidence)> PredictAsync(string labelRaw, CancellationToken ct)
        => Task.FromResult<(Guid?, Guid?, float)>((null, null, 0.0f));

    public Task FeedbackAsync(string labelRaw, string? brand, Guid typeId, Guid? categoryId, CancellationToken ct)
        => Task.CompletedTask;
}
