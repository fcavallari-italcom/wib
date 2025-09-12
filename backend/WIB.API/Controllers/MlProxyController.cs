using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WIB.API.Controllers;

[ApiController]
[Route("ml")]
public class MlProxyController : ControllerBase
{
    private readonly HttpClient _client;

    public MlProxyController(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("ml");
    }

    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromBody] object body)
    {
        var content = new StringContent(body.ToString() ?? "{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("predict", content);
        return Content(await response.Content.ReadAsStringAsync(), "application/json");
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> Feedback([FromBody] object body)
    {
        var content = new StringContent(body.ToString() ?? "{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("feedback", content);
        return Content(await response.Content.ReadAsStringAsync(), "application/json");
    }

    [HttpPost("train")]
    public async Task<IActionResult> Train()
    {
        var response = await _client.PostAsync("train", null);
        return Content(await response.Content.ReadAsStringAsync(), "application/json");
    }
}
