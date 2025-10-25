using Godot;
using System;
using System.Text.Json;

public class SaveData
{
    public int SchemaVersion { get; set; } = 1;
    public double Money { get; set; }
    public double LinesOfCode { get; set; }
    public double IncomePerSecond { get; set; }
    public int ClickPower { get; set; }
    public long LastSavedUnix { get; set; }
}

public static class SaveService
{
    // Writes the whole state to disk (user sandbox)
    public static void Save(string path, CurrencyManager cm)
    {
        try
        {
            var data = cm.ToSave();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreString(json);
            // FileAccess disposed by using
        }
        catch (Exception e)
        {
            GD.PushWarning($"Save failed: {e.Message}");
        }
    }

    // Reads and parses the file (or returns null if not found / invalid)
    public static SaveData Load(string path)
    {
        try
        {
            if (!FileAccess.FileExists(path)) return null;

            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var data = JsonSerializer.Deserialize<SaveData>(json);
            return data;
        }
        catch (Exception e)
        {
            GD.PushWarning($"Load failed: {e.Message}");
            return null;
        }
    }
}