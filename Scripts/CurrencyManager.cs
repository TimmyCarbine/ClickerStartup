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
    public const double PCT_PER_IC = 5.0;         // percentage points per IC (5%)
    public const double FRAC_PER_IC = 0.05;       // fraction per IC (0.05)
    public double GlobalBonusFraction => InvestorCapital * FRAC_PER_IC;
    public double GlobalBonusPercentPoints => InvestorCapital * PCT_PER_IC;
    public double GlobalMult => 1.0 + GlobalBonusFraction;
    private double ExcessMoney => Math.Max(0.0, Money - MaxMoneyEarned);
    private double Chunks => ExcessMoney / IC_BASE_CHUNK;

    public double ClickGainPerPress => (BaseClickPower + ClickFlat) * ClickMult * GlobalMult;
    public double CurrentIncomePerSec => (BaseIncomePerSecond + IncomeFlat) * IncomeMult * GlobalMult;

    // === IC CURVE ===
    public const double IC_BASE_CHUNK = 10_000.0;                           // 10k beyond record remains the first milestone
    public const double IC_SOFT_CAP = 100.0;                                // Soft cap of IC gained per run
    private const double TARGET_AT_P1 = 2.0;                                // How many IC gained at prestige 1
    private double IC_ALPHA => -Math.Log(1.0 - TARGET_AT_P1 / IC_SOFT_CAP); // Derived steepness so that raw(p=1) ≈ TARGET_AT_P1 regardless of IC_SOFT_CAP

    // Optional “scale with how strong this run is” (set to 0 to disable).
    // We normalise by a reference strength so the factor is ~1.0 midgame.
    private const double STRENGTH_WEIGHT = 0.35;        // 0.0 = off; try 0.15 .. 0.50 for mild → noticeable effect
    private const double REF_CLICK       = 10.0;        // reference click power for normalisation
    private const double REF_INCOME = 5.0;              // reference income/sec for normalisation

    private double FirstIcTarget => MaxMoneyEarned + IC_BASE_CHUNK;
    private double PercentOverRecordBeyondGate
    {
        get
        {
            // Use the larger of record and the first target so p is small at the gate on fresh runs
            double norm = Math.Max(FirstIcTarget, MaxMoneyEarned); // e.g., 10k when record=0
            double p = (Money - FirstIcTarget) / Math.Max(1e-9, norm);
            return Math.Max(0.0, p);
        }
    }

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

    // === PRESTIGE FLOW ===
    private double RunStrengthFactor()
    {
        double clickRatio = ClickGainPerPress / Math.Max(1e-9, REF_CLICK);
        double incomeRatio = CurrentIncomePerSec / Math.Max(1e-9, REF_INCOME);
        double adj = Math.Sqrt(Math.Max(0.0, clickRatio)) + Math.Sqrt(Math.Max(0.0, incomeRatio)) - 2.0;
        return Math.Max(0.25, 1.0 + STRENGTH_WEIGHT * adj);
    }
    private double IcRawFromPercent(double p)
    {
        if (p <= 1e-12) return 0.0;
        // raw = K * (1 - exp(-alpha * p))
        double raw = IC_SOFT_CAP * (1.0 - Math.Exp(-IC_ALPHA * p));
        return Math.Clamp(raw, 0.0, IC_SOFT_CAP);
    }
    private double PercentNeededForRaw(double rawTarget)
    {
        double s = Math.Clamp(rawTarget / IC_SOFT_CAP, 1e-9, 1.0 - 1e-9);
        return -Math.Log(1.0 - s) / IC_ALPHA;
    }
    // === PREVIEW / NEXT TARGET / DO PRESTIGE ===

    public long PreviewInvestorGain()
    {
        if (Money < FirstIcTarget - 1e-9) return 0;

        double p = PercentOverRecordBeyondGate;
        double raw = IcRawFromPercent(p);

        // Guarantee +1 as soon as you cross the gate; thereafter floor(raw)
        long whole = (long)Math.Floor(raw + 1e-9);
        return Math.Max(1, whole);
    }
    public double NextIcTargetMoney
    {
        get
        {
            long currentWhole = PreviewInvestorGain();
            if (currentWhole <= 0) return FirstIcTarget;

            double nextTargetIc = currentWhole + 1.0;
            if (nextTargetIc >= IC_SOFT_CAP - 1e-9)
                return double.PositiveInfinity;

            double pNeeded = PercentNeededForRaw(nextTargetIc);

            // IMPORTANT: use the same normaliser as the preview path
            double norm = Math.Max(FirstIcTarget, MaxMoneyEarned);
            double requiredExcessBeyondGate = pNeeded * norm;

            return FirstIcTarget + requiredExcessBeyondGate;
        }
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
        MaxMoneyEarned = 0.0;
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