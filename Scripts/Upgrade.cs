using System.Text.Json.Serialization;

public class Upgrade
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("cost")] public double BaseCost { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } // "click_flat" | "click_mult" | "income_flat" | "income_mult"
    [JsonPropertyName("amount")] public double Amount { get; set; }
    [JsonPropertyName("limit")] public int Limit { get; set; }
    [JsonPropertyName("cost_curve")] public string CostCurve { get; set; } = "geometric"; // "geometric" | "linear" | "none"
    [JsonPropertyName("growth")] public double Growth { get; set; } = 1.15; // used for geometric
    [JsonPropertyName("step")] public double Step { get; set; } = 0; // used for linear

    public int Purchases { get; set; } = 0;

    public bool IsLimited => Limit >= 0 && Purchases >= Limit;

    public double CurrentCost => CostCurve switch
    {
        "none" => BaseCost,
        "linear" => BaseCost + Step * Purchases,
        _ => BaseCost * System.Math.Pow(Growth, Purchases) // geometric default
    };

    public void ApplyCount(CurrencyManager cm, int count) =>
        UpgradeEffects.ApplyCount(cm, this, count);
}