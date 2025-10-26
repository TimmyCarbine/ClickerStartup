using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class SaveData
{
    public int SchemaVersion { get; set; } = 2;
    public long LastSavedUnix { get; set; }

    // === CORE CURRENCIES ===
    public double Money { get; set; }
    public double LinesOfCode { get; set; }

    // === PRESTIGE / GLOBAL ===
    public double InvestorCapital { get; set; }

    // === UPGRADES ===
    public Dictionary<string, int> UpgradeCounts { get; set; } = new();

}

public static class SaveService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static void SaveAll(string path, CurrencyManager cm, UpgradeManager um)
    {
        try
        {
            var data = new SaveData
            {
                SchemaVersion = 2,
                Money = cm.Money,
                LinesOfCode = cm.LinesOfCode,
                InvestorCapital = cm.InvestorCapital,
                UpgradeCounts = um.Upgrades.ToDictionary(u => u.Id, u => u.Purchases),
                LastSavedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreString(json);
            GD.Print("[Save] OK");
        }
        catch (Exception e)
        {
            GD.PushWarning($"[Save] Failed: {e.Message}");
        }
    }

    public static bool LoadAll(string path, CurrencyManager cm, UpgradeManager um)
    {
        try
        {
            if (!FileAccess.FileExists(path)) return false;

            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var data = JsonSerializer.Deserialize<SaveData>(json, _jsonOptions);
            if (data == null) return false;

            cm.SetMoney(data.Money);
            cm.SetLinesOfCode(data.LinesOfCode);
            cm.SetInvestorCapital(data.InvestorCapital);

            foreach (var u in um.Upgrades)
            {
                u.Purchases = 0;
                if (data.UpgradeCounts != null && data.UpgradeCounts.TryGetValue(u.Id, out var count))
                    u.Purchases = Math.Max(0, count);
            }
            
            um.ReapplyAll(cm);
            GD.Print("[Load] OK");
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning($"[Load] Failed: {e.Message}");
            return false;
        }
    }

    public static void Delete(string path)
    {
        try
        {
            if(FileAccess.FileExists(path))
            {
                var dir = DirAccess.Open("user://");
                if(dir != null)
                {
                    dir.Remove(path.Replace("user://", ""));
                    GD.Print($"[Save] Deleted: {path}");
                }
            }
        }
        catch (Exception e)
        {
            GD.PushWarning($"[Delete] Failed: {e.Message}");
        }
    }
}