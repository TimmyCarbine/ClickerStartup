using System.Text.Json.Serialization;

public class Upgrade
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("cost")]
    public double Cost { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } // "click_power", "income_per_sec", "income_multiplier"

    [JsonPropertyName("amount")]
    public double Amount { get; set; }
    
    public bool Purchased { get; set; } = false;
}