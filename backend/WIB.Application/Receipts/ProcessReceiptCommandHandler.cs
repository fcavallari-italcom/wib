using WIB.Application.Interfaces;

namespace WIB.Application.Receipts;

public class ProcessReceiptCommandHandler
{
    private readonly IOcrClient _ocr;
    private readonly IKieClient _kie;
    private readonly IProductClassifier _classifier;
    private readonly IReceiptStorage _storage;

    public ProcessReceiptCommandHandler(IOcrClient ocr, IKieClient kie, IProductClassifier classifier, IReceiptStorage storage)
    {
        _ocr = ocr;
        _kie = kie;
        _classifier = classifier;
        _storage = storage;
    }

    public async Task Handle(ProcessReceiptCommand command, CancellationToken ct)
    {
        var ocrResult = await _ocr.ExtractAsync(command.Image, ct);
        var kieResult = await _kie.ExtractFieldsAsync(ocrResult, ct);
        // TODO: parse kieResult and classify lines
        await _storage.SaveAsync(new WIB.Domain.Receipt(), ct);
    }
}
