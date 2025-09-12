using WIB.Application.Interfaces;
using WIB.Application.Models;

namespace WIB.Infrastructure.Clients;

public class ProductClassifier : IProductClassifier
{
    public Task<ClassificationResult> PredictAsync(string labelRaw, string? brand, CancellationToken ct)
        => Task.FromResult(new ClassificationResult(Array.Empty<ClassificationCandidate>(), Array.Empty<ClassificationCandidate>()));

    public Task FeedbackAsync(string labelRaw, string? brand, Guid finalTypeId, Guid? finalCategoryId, CancellationToken ct)
        => Task.CompletedTask;
}
