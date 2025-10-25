using System;

public class CurrencyManager
{
    // === STATE ===
    public double Money { get; private set; } = 0.0;
    public double LinesOfCode { get; private set; } = 0.0;
    public double IncomePerSecond { get; private set; } = 0.0;
    public int ClickPower { get; set; } = 1;

    // === ACTIONS ===
    public void AddOnClick()
    {
        // Click adds to Money and LoC
        Money += ClickPower;
        LinesOfCode += 1;
        ClampNegatives();
    }

    public void AddIncomePerSecond(double amount)
    {
        IncomePerSecond += amount;
        ClampNegatives();
    }

    public void ApplyPassiveTick(double seconds)
    {
        Money += IncomePerSecond * seconds;
        ClampNegatives();
    }

    public bool TrySpend(double amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            ClampNegatives();
            return true;
        }
        return false;
    }

    // === SAVE/LOAD HELPERS ===
    public SaveData ToSave()
    {
        return new SaveData
        {
            SchemaVersion = 1,
            Money = Money,
            LinesOfCode = LinesOfCode,
            IncomePerSecond = IncomePerSecond,
            ClickPower = ClickPower,
            LastSavedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void ApplySave(SaveData data)
    {
        Money = data.Money;
        LinesOfCode = data.LinesOfCode;
        IncomePerSecond = data.IncomePerSecond;
        ClickPower = data.ClickPower;
        ClampNegatives();
    }

    // === CURRENCY HELPERS ===
    private void ClampNegatives()
    {
        if (Money < 0) Money = 0;
        if (LinesOfCode < 0) LinesOfCode = 0;
        if (IncomePerSecond < 0) LinesOfCode = 0;
        if (ClickPower < 1) ClickPower = 1;
    }
}