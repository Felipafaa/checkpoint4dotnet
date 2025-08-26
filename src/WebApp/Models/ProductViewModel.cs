using System.Text.Json.Serialization;

namespace WebApp.Models;

public class ProductViewModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; }
}