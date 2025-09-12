using WIB.Application.Models;

namespace WIB.Application.Interfaces;

public interface IProductClassifier
{
    Task<ClassificationResult> PredictAsync(string labelRaw, string? brand, CancellationToken ct);
    Task FeedbackAsync(string labelRaw, string? brand, Guid finalTypeId, Guid? finalCategoryId, CancellationToken ct);
}
