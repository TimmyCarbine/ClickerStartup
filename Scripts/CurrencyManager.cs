using System;

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
    public double GlobalMult => 1.0 + 0.05 * InvestorCapital;
    public double InvestorCapital { get; private set; } = 0.0;

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

    // === CURRENCY HELPERS ===
    private void ClampNegatives()
    {
        if (Money < 0) Money = 0;
        if (LinesOfCode < 0) LinesOfCode = 0;
        if (BaseClickPower < 1) BaseClickPower = 1;
        if (BaseIncomePerSecond < 0) BaseIncomePerSecond = 0;
        if (ClickFlat < 0) ClickFlat = 0;
        if (IncomeFlat < 0) IncomeFlat = 0;
    }

    public void ResetStatsKeepRunAndPrestige()
    {
        BaseClickPower = 1;
        BaseIncomePerSecond = 0;

        ClickFlat = 0;
        IncomeFlat = 0;

        ClickMult = 1.0;
        IncomeMult = 1.0;
    }

    public void ResetAll()
    {
        Money = 0;
        LinesOfCode = 0;
        BaseClickPower = 1;
        BaseIncomePerSecond = 0;
        ClickFlat = 0;
        IncomeFlat = 0;
        ClickMult = 1;
        IncomeMult = 1;
        ClampNegatives();
    }
}