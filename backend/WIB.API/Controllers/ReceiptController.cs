using Microsoft.AspNetCore.Mvc;
using WIB.Application.Receipts;

namespace WIB.API.Controllers;

[ApiController]
[Route("receipts")]
public class ReceiptController : ControllerBase
{
    private readonly ProcessReceiptCommandHandler _handler;

    public ReceiptController(ProcessReceiptCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        await _handler.Handle(new ProcessReceiptCommand(file.OpenReadStream()), ct);
        return Accepted();
    }
}
