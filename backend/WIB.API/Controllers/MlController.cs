using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace WIB.API.Controllers;

[ApiController]
[Route("ml")]
public class MlController : ControllerBase
{
    private readonly HttpClient _client;

    public MlController(HttpClient client)
    {
        _client = client;
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> Suggestions([FromQuery] string labelRaw)
    {
        var resp = await _client.PostAsJsonAsync("/predict", new { labelRaw });
        var body = await resp.Content.ReadAsStringAsync();
        return Content(body, resp.Content.Headers.ContentType?.MediaType);
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback([FromBody] object payload)
    {
        var resp = await _client.PostAsJsonAsync("/feedback", payload);
        var body = await resp.Content.ReadAsStringAsync();
        return Content(body, resp.Content.Headers.ContentType?.MediaType);
    }
}
