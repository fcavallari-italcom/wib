using Microsoft.AspNetCore.Mvc;
using WIB.Application.Interfaces;

namespace WIB.API.Controllers;

[ApiController]
[Route("ml")]
public class MlProxyController : ControllerBase
{
    private readonly IProductClassifier _classifier;

    public MlProxyController(IProductClassifier classifier)
    {
        _classifier = classifier;
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string labelRaw, [FromQuery] string? brand, CancellationToken ct)
    {
        var result = await _classifier.PredictAsync(labelRaw, brand, ct);
        return Ok(result);
    }

    public record FeedbackRequest(string LabelRaw, string? Brand, Guid FinalTypeId, Guid? FinalCategoryId);

    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback([FromBody] FeedbackRequest request, CancellationToken ct)
    {
        await _classifier.FeedbackAsync(request.LabelRaw, request.Brand, request.FinalTypeId, request.FinalCategoryId, ct);
        return Accepted();
    }
}
