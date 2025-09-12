using Microsoft.AspNetCore.Mvc;

namespace WIB.API.Controllers;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpGet("spending")]
    public IActionResult GetSpending(DateTime from, DateTime to) => Ok(new { from, to });

    [HttpGet("price-history")]
    public IActionResult GetPriceHistory(Guid productId, Guid? storeId = null) => Ok(new { productId, storeId });
}
