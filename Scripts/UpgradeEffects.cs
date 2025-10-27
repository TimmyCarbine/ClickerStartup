using Godot;
using System;
using System.Collections.Generic;

public static class UpgradeEffects
{
    private static readonly Dictionary<string, Action<CurrencyManager, Upgrade, int>> HANDLERS = new(StringComparer.OrdinalIgnoreCase)
    {
        ["click_flat"] = (cm, u, c) => cm.AddClickFlat((int)(u.Amount * c)),
        ["click_mult"] = (cm, u, c) => cm.MultiplyClick(Math.Pow(1 + u.Amount, c)),
        ["income_flat"] = (cm, u, c) => cm.AddBaseIncome(u.Amount * c),
        ["income_mult"] = (cm, u, c) => cm.MultiplyIncome(Math.Pow(1 + u.Amount, c))
    };

    public static void ApplyCount(CurrencyManager cm, Upgrade u, int count)
    {
        if (count <= 0) return;
        if (!HANDLERS.TryGetValue(u.Type, out var fn))
        {
            GD.PushWarning($"Unknown upgrade type: {u.Type}");
            return;
        }
        fn(cm, u, count);
    }
}