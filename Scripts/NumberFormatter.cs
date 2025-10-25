using System;

public static class NumberFormatter
{
    public static string Format(double n)
    {
        // Whole numbers up to 999
        if (Math.Abs(n) < 1_000d) return n.ToString("0");
        // Thousands
        if (Math.Abs(n) < 1_000_000d) return (n / 1_000d).ToString("0.#") + "K";
        // Millions
        if (Math.Abs(n) < 1_000_000_000d) return (n / 1_000_000d).ToString("0.#") + "M";
        // Billions
        if (Math.Abs(n) < 1_000_000_000_000d) return (n / 1_000_000_000d).ToString("0.#") + "B";
        // Trillions +
        return (n / 1_000_000_000_000d).ToString("0.#") + "T";
    }
}