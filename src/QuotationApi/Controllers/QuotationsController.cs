using Microsoft.AspNetCore.Mvc;

namespace QuotationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotationsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public QuotationsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("usd-brl")]
    public async Task<IActionResult> GetUsdBrlQuotation()
    {
        var client = _httpClientFactory.CreateClient("AwesomeApiClient");
        var response = await client.GetAsync("json/last/USD-BRL");

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var content = await response.Content.ReadFromJsonAsync<object>();
        return Ok(content);
    }
}