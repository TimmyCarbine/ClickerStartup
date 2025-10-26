using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;

public class UpgradeManager
{
    public List<Upgrade> Upgrades { get; private set; } = new();

    public bool TryBuy(Upgrade u, CurrencyManager cm)
    {
        if (u.IsLimited) return false;

        var cost = u.CurrentCost;
        if (!cm.TrySpend(cost)) return false;

        u.Purchases += 1;

        switch (u.Type)
        {
            case "click_flat":
                cm.AddClickFlat((int)u.Amount);
                break;
            case "click_mult":
                cm.MultiplyClick(1 + u.Amount);
                break;
            case "income_flat":
                cm.AddBaseIncome(u.Amount);
                break;
            case "income_mult":
                cm.MultiplyIncome(1 + u.Amount);
                break;
            default:
                GD.PushWarning($"Unknown upgrade type: {u.Type}");
                break;
        }
        return true;
    }

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

    public void ReapplyAll(CurrencyManager cm)
    {
        cm.ResetStatsKeepRunAndPrestige();

        foreach (var u in Upgrades)
        {
            if (u.Purchases <= 0) continue;

            switch (u.Type)
            {
                case "click_flat":
                    cm.AddClickFlat((int)u.Amount);
                    break;
                case "click_mult":
                    cm.MultiplyClick(1 + u.Amount);
                    break;
                case "income_flat":
                    cm.AddBaseIncome(u.Amount);
                    break;
                case "income_mult":
                    cm.MultiplyIncome(1 + u.Amount);
                    break;
                default:
                    GD.PushWarning($"Unknown upgrade type: {u.Type}");
                    break;
            }
        }
    }
}