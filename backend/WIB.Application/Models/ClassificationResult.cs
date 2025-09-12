namespace WIB.Application.Models;

public record ClassificationCandidate(Guid Id, float Confidence);

public record ClassificationResult(
    IReadOnlyCollection<ClassificationCandidate> TypeCandidates,
    IReadOnlyCollection<ClassificationCandidate> CategoryCandidates);
