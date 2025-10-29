using Godot;
using System;
using System.Collections.Generic;

public class CurrencyManager
{
    // === STATE ===
    public double Money { get; private set; } = 0.0;
    public double LinesOfCode { get; private set; } = 0.0;

    // === BASE VALUES ===
    public int BaseClickPower { get; private set; } = 1;
    public double BaseIncomePerSecond { get; private set; } = 0.0;

    // === ADDITIVE BONUSES ===
    public int ClickFlat { get; private set; } = 0;
    public double IncomeFlat { get; private set; } = 0.0;

    // === MULTIPLICATIVE BONUSES ===
    public double ClickMult { get; private set; } = 1.0;
    public double IncomeMult { get; private set; } = 1.0;

    // === PRESTIGE ===
    public double MaxMoneyEarned { get; private set; } = 0.0;
    public double InvestorCapital { get; private set; } = 0.0;
    public double GlobalMult => 1.0 + InvestorCapital * 0.05;
    public double NextIcTargetMoney => MaxMoneyEarned + 10_000.0;

    public double ClickGainPerPress => (BaseClickPower + ClickFlat) * ClickMult * GlobalMult;
    public double CurrentIncomePerSec => (BaseIncomePerSecond + IncomeFlat) * IncomeMult * GlobalMult;

    // === ACTIONS ===
    public void AddOnClick()
    {
        // Click adds to Money and LoC
        Money += ClickGainPerPress;
        LinesOfCode += 1;
        ClampNegatives();
    }
    public void AddClickFlat(int amount)
    {
        ClickFlat += amount;
        ClampNegatives();
    }
    public void MultiplyClick(double factor)
    {
        ClickMult *= factor;
        ClampNegatives();
    }
    public void AddBaseIncome(double amount)
    {
        BaseIncomePerSecond += amount;
        ClampNegatives();
    }
    public void MultiplyIncome(double factor)
    {
        IncomeMult *= factor;
        ClampNegatives();
    }
    public void SetMoney(double amount)
    {
        Money = amount;
        ClampNegatives();
    }
    public void SetLinesOfCode(double amount)
    {
        LinesOfCode = amount;
        ClampNegatives();
    }
    public void SetMaxMoneyEarned(double amount) => MaxMoneyEarned = Math.Max(0.0, amount);
    public void SetInvestorCapital(double amount)
    {
        InvestorCapital = amount;
        ClampNegatives();
    }
    public void ApplyPassiveTick(double seconds)
    {
        Money += CurrentIncomePerSec * seconds;
        ClampNegatives();
    }
    public bool TrySpend(double amount)
    {
        if (Money < amount) return false;
        
        Money -= amount;
        ClampNegatives();
        return true;
    }

    // === PRESTIGE FLOW (for completeness) ===

    public long PreviewInvestorGain()
    {
        double excess = Money - MaxMoneyEarned;
        if (excess < 0) excess = 0;
        return (long)Math.Floor(excess / 10_000.0);
    }
    public long DoPrestige()
    {
        long gained = PreviewInvestorGain();
        if (gained > 0)
        {
            InvestorCapital += gained;
            if (Money > MaxMoneyEarned) MaxMoneyEarned = Money;
        }
        ResetRunButKeepPrestige();
        return gained;
    }

    // === CURRENCY HELPERS ===
    private void ClampNegatives()
    {
        if (Money < 0) Money = 0;
        if (LinesOfCode < 0) LinesOfCode = 0;
        if (BaseIncomePerSecond < 0) BaseIncomePerSecond = 0;
        if (IncomeFlat < 0) IncomeFlat = 0;
        if (ClickFlat < 0) ClickFlat = 0;
        if (ClickMult < 0) ClickMult = 1.0;
        if (IncomeMult < 0) IncomeMult = 1.0;
    }

    // === PUBLIC API ===

    /// Reset the current run (currencies + non-prestige stats), keep InvestorCapital.
    public void ResetRunButKeepPrestige()
    {
        ResetCurrencies();
        ResetNonPrestigeStats();
        ClampNegatives();
    }

    /// Total nuke: also clears InvestorCapital (so GlobalMult returns to 1.0).
    public void ResetAll()
    {
        ResetRunButKeepPrestige();
        InvestorCapital = 0.0;
    }

    public void RebuildStatsFrom(IEnumerable<Upgrade> upgrades)
    {
        // Do NOT touch currencies or prestige/meta here
        ResetNonPrestigeStats(); // private helper you already have

        // Reapply effects from purchase counts
        foreach (var u in upgrades)
        {
            u.ApplyCount(this, u.Purchases);
        }
    }

    // === PRIVATE HELPERS ===

    private void ResetCurrencies()
    {
        Money = 0;
        LinesOfCode = 0;
    }

    private void ResetNonPrestigeStats()
    {
        // base
        BaseClickPower = 1;
        BaseIncomePerSecond = 0;

        // additive
        ClickFlat = 0;
        IncomeFlat = 0;

        // multiplicative (non-prestige only)
        ClickMult = 1.0;
        IncomeMult = 1.0;
    }
}