using Microsoft.AspNetCore.Mvc;

namespace WIB.API.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok" });
}
