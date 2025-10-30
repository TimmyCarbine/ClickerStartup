using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;

public class UpgradeManager
{
    public List<Upgrade> Upgrades { get; private set; } = new();

    public Upgrade GetById(string id) => Upgrades.Find(x => x.Id == id);

    public bool IsUnlocked(Upgrade u)
    {
        if (string.IsNullOrWhiteSpace(u.RequiresId)) return true;

        var prereq = GetById(u.RequiresId);
        if (prereq == null) return true;

        if (string.Equals(u.Requires, "maxed", StringComparison.OrdinalIgnoreCase))
        {
            if (prereq.Limit < 0) return false;
            return prereq.Purchases >= prereq.Limit;
        }

        if (u.RequiresMin.HasValue) return prereq.Purchases >= u.RequiresMin.Value;

        return true;
    }

    public bool TryBuy(Upgrade u, CurrencyManager cm, int requestedCount, out int boughtCount)
    {
        boughtCount = 0;
        if (u.IsLimited && u.RemainingPurchases <= 0) return false;

        int count = requestedCount < 0 ? u.GetMaxAffordable(cm.Money)
                                    : Math.Min(requestedCount, u.RemainingPurchases);
        if (count <= 0) return false;

        double total = u.TotalCostFor(count);
        if (!cm.TrySpend(total)) return false;

        u.Purchases += count;
        u.ApplyCount(cm, count);
        boughtCount = count;
        return true;
    }

    public bool TryBuy(Upgrade u, CurrencyManager cm) => TryBuy(u, cm, 1, out _);

    public bool LoadUpgrades(string path)
    {
        try
        {
            if (!FileAccess.FileExists(path))
            {
                GD.PushWarning($"Upgrades file not found: {path}");
                Upgrades = new();
                return false;
            }
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            Upgrades = JsonSerializer.Deserialize<List<Upgrade>>(json);
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning($"Failed to load upgrades: {e.Message}");
            Upgrades = new();
            return false;
        }
    }

    public void ReapplyAll(CurrencyManager cm) => cm.RebuildStatsFrom(Upgrades);
}