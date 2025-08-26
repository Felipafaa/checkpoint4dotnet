using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public HomeController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var productClient = _clientFactory.CreateClient("ProductApi");
        try
        {
            var productsResponse = await productClient.GetAsync("/api/products");
            if (productsResponse.IsSuccessStatusCode)
            {
                var productsStream = await productsResponse.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                ViewBag.Products = await JsonSerializer.DeserializeAsync<IEnumerable<ProductViewModel>>(productsStream, options);
            }
            else
            {
                ViewBag.ApiError = $"ProductAPI retornou: {productsResponse.ReasonPhrase}";
                ViewBag.Products = Enumerable.Empty<ProductViewModel>();
            }
        }
        catch (Exception ex)
        {
            ViewBag.ApiError = $"Não foi possível conectar à ProductAPI: {ex.Message}";
            ViewBag.Products = Enumerable.Empty<ProductViewModel>();
        }

        var quotationClient = _clientFactory.CreateClient("QuotationApi");
        try
        {
            var quotationResponse = await quotationClient.GetAsync("/api/quotations/usd-brl");
            if (quotationResponse.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(await quotationResponse.Content.ReadAsStringAsync());
                ViewBag.Quotation = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                ViewBag.Quotation = "Cotação indisponível (QuotationAPI retornou erro).";
            }
        }
        catch (Exception ex)
        {
            ViewBag.Quotation = $"Não foi possível conectar à QuotationAPI: {ex.Message}";
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}