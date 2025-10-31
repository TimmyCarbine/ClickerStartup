using System;
using System.Diagnostics.Contracts;

public static class NumberFormatter
{
    public enum Mode { Short, Scientific }
    public static Mode CurrentMode { get; set; } = Mode.Short;

    public static string Format(double v) => CurrentMode == Mode.Scientific ? FormatSci(v) : FormatShort(v);
    public static string FormatPercent(double v) => CurrentMode == Mode.Scientific ? FormatSciPercent(v) : FormatShortPercent(v);

    public static string FormatShort(double n)
    {
        if (double.IsNaN(n)) return "0";
        if (double.IsPositiveInfinity(n)) return "∞";
        if (double.IsNegativeInfinity(n)) return "-∞";
        // Whole numbers up to 999
        if (Math.Abs(n) < 1_000d) return n.ToString("0");
        // Thousands
        if (Math.Abs(n) < 1_000_000d) return (n / 1_000d).ToString("0.#") + " K";
        // Millions
        if (Math.Abs(n) < 1_000_000_000d) return (n / 1_000_000d).ToString("0.#") + " M";
        // Billions
        if (Math.Abs(n) < 1_000_000_000_000d) return (n / 1_000_000_000d).ToString("0.#") + " B";
        // Trillions
        if(Math.Abs(n) < 1_000_000_000_000_000d) return (n / 1_000_000_000_000d).ToString("0.#") + " T";
        // Quadrillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000d).ToString("0.#") + " Qa";
        // Quintrillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000d).ToString("0.#") + " Qi";
        // Sextillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000d).ToString("0.#") + " Sx";
        // Septillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000_000d).ToString("0.#") + " Sp";
        // Octillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000_000_000d).ToString("0.#") + " Oc";
        // Nonillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000_000_000_000d).ToString("0.#") + " No";
        // Decillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000_000_000_000_000d).ToString("0.#") + " Dc";
        // Undecillions
        if(Math.Abs(n) < 1_000_000_000_000_000_000_000_000_000_000_000_000_000d) return (n / 1_000_000_000_000_000_000_000_000_000_000_000_000d).ToString("0.#") + " UDc";
        // Duodecillions +
        return (n / 1_000_000_000_000_000_000_000_000_000_000_000_000_000d).ToString("0.#") + " DDc";
    }
    public static string FormatShortPercent(double value)
    {
        if (double.IsNaN(value)) return "0 %";
        if (double.IsPositiveInfinity(value)) return "∞ %";
        if (double.IsNegativeInfinity(value)) return "-∞ %";
        if (value < 1000) return $"{value:0} %";
        if (value < 1_000_000) return $"{value / 1_000:0.##} K %";
        if (value < 1_000_000_000) return $"{value / 1_000_000:0.##} M %";
        if (value < 1_000_000_000_000) return $"{value / 1_000_000_000:0.##} B %";
        if (value < 1_000_000_000_000_000d) return $"{value / 1_000_000_000_000:0.##} T %";
        if (value < 1_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000d:0.##} Qa %";
        if (value < 1_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000d:0.##} Qi %";
        if (value < 1_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000d:0.##} Sx %";
        if (value < 1_000_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000_000d:0.##} Sp %";
        if (value < 1_000_000_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000_000_000d:0.##} Oc %";
        if (value < 1_000_000_000_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000_000_000_000d:0.##} No %";
        if (value < 1_000_000_000_000_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000_000_000_000_000d:0.##} Dc %";
        if (value < 1_000_000_000_000_000_000_000_000_000_000_000_000_000d) return $"{value / 1_000_000_000_000_000_000_000_000_000_000_000_000d:0.##} UDc %";
        return $"{value / 1_000_000_000_000_000_000_000_000_000_000_000_000_000d:0.##} DDc %";
    }
    private static string FormatSci(double n)
    {
        if (double.IsNaN(n)) return "0";
        if (double.IsPositiveInfinity(n)) return "∞";
        if (double.IsNegativeInfinity(n)) return "-∞";
        if (Math.Abs(n) < 1000) return n.ToString("0.##");
        return n.ToString("0.###e+0");
    }
    private static string FormatSciPercent(double value)
    {
        if (double.IsNaN(value)) return "0 %";
        if (double.IsPositiveInfinity(value)) return "∞ %";
        if (double.IsNegativeInfinity(value)) return "-∞ %";
        if (value < 1000) return $"{value:0} %";
        return value.ToString("0.###e+0 %");
    }

}