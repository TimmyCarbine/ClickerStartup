using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;

public class UpgradeManager
{
    public List<Upgrade> Upgrades { get; private set; } = new();

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

    public IEnumerable<Upgrade> GetAvailable(CurrencyManager cm)
    {
        foreach (var u in Upgrades)
            if (!u.Purchased && cm.Money >= u.Cost)
                yield return u;
    }

    public void ApplyUpgrade(Upgrade u, CurrencyManager cm)
    {
        if (u.Purchased) return;
        if (cm.TrySpend(u.Cost))
        {
            switch (u.Type)
            {
                case "click_power":
                    cm.AddClickPower((int)u.Amount);
                    break;
                case "income_per_sec":
                    cm.AddIncomePerSecond(u.Amount);
                    break;
                case "income_multiplier":
                    cm.IncomeMultiplier *= (1 + u.Amount);
                    break;
                default:
                    GD.PushWarning($"Unknown upgrade type: {u.Type}");
                    break;
            }
        }
        u.Purchased = true;
    }
}