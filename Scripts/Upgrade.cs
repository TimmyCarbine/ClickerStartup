using System;
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
    // === PREREQUISITES (optional) ===
    // Show this upgrade only when another upgrade meets a condition
    [JsonPropertyName("requires_id")] public string RequiresId { get; set; } // e.g., "marketing_1"
    // If set, require the prerequisite to have at least this many purchases
    [JsonPropertyName("requires_min")] public int? RequiresMin { get; set; }
    // If "maxed", require the prerequisite to be at it's purchase limit
    [JsonPropertyName("requires")] public string Requires { get; set; } // "maxed" | null

    public int Purchases { get; set; } = 0;
    
    public bool IsLimited => Limit >= 0 && Purchases >= Limit;

    public double CurrentCost => CostCurve switch
    {
        "none" => BaseCost,
        "linear" => BaseCost + Step * Purchases,
        _ => BaseCost * System.Math.Pow(Growth, Purchases) // geometric default
    };

    public int RemainingPurchases
    {
        get
        {
            if (Limit < 0) return int.MaxValue;
            return Math.Max(0, Limit - Purchases);
        }
    }

    public double TotalCostFor(int count)
    {
        count = Math.Max(0, Math.Min(count, RemainingPurchases));
        if (count == 0) return 0;

        int p = Purchases;

        switch (CostCurve)
        {
            case "none":
                return (p < Limit || Limit < 0) ? BaseCost : 0;

            case "linear":
                return count * (BaseCost + Step * p) + Step * (count - 1) * count / 2.0;

            default:
                double g = Growth <= 0 ? 1.0 : Growth;
                if (Math.Abs(g - 1.0) < 1e-12) return BaseCost * count;
                double start = BaseCost * Math.Pow(g, p);
                return start * (Math.Pow(g, count) - 1.0) / (g - 1.0);
        }
    }

    public int GetMaxAffordable(double money)
    {
        int avail = RemainingPurchases;
        if (avail <= 0 || money <= 0) return 0;

        int p = Purchases;

        switch (CostCurve)
        {
            case "none":
                // One-off: at most 1 if not bought yet.
                return ((Limit < 0 || p < Limit) && money >= BaseCost) ? 1 : 0;

            case "linear":
                // Arithmetic series:
                // term_k = (BaseCost + Step*(p+k)), k=0..n-1
                // sum(n) = n*(BaseCost + Step*p) + Step * n*(n-1)/2 <= money
                // => (Step/2) n^2 + (BaseCost + Step*p - Step/2) n - money <= 0
                double A = Step / 2.0;
                double B = (BaseCost + Step * p) - Step / 2.0;
                double C = -money;

                if (Math.Abs(A) < 1e-12)
                {
                    // Degenerate linear (Step≈0): simply floor(money / currentCost)
                    double c0 = BaseCost + Step * p;
                    int n0 = (int)Math.Floor(money / Math.Max(1e-12, c0));
                    return Math.Max(0, Math.Min(avail, n0));
                }

                double disc = B * B - 4 * A * C;
                if (disc < 0) return 0;
                int nLin = (int)Math.Floor((-B + Math.Sqrt(disc)) / (2 * A));
                return Math.Max(0, Math.Min(avail, nLin));

            default: // "geometric" (or unknown → treat as geometric)
                double g = (Growth <= 0) ? 1.0 : Growth;
                double start = BaseCost * Math.Pow(g, p);

                if (Math.Abs(g - 1.0) < 1e-12)
                {
                    // Degenerate geometric (g≈1): flat pricing
                    int nFlat = (int)Math.Floor(money / Math.Max(1e-12, start));
                    return Math.Max(0, Math.Min(avail, nFlat));
                }

                // Sum(n) = start * (g^n - 1) / (g - 1) <= money
                // g^n <= 1 + money*(g - 1)/start
                double rhs = 1.0 + (money * (g - 1.0)) / Math.Max(1e-300, start);
                if (rhs <= 1.0) return 0;

                int nGeo = (int)Math.Floor(Math.Log(rhs) / Math.Log(g));
                return Math.Max(0, Math.Min(avail, nGeo));
        }
    }

    public void ApplyCount(CurrencyManager cm, int count) =>
        UpgradeEffects.ApplyCount(cm, this, count);
}