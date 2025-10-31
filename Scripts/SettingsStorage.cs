using Godot;
using System;
using System.Text.Json;

public static class SettingsStorage
{
    private const string PATH = "user://settings.json";
    private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public static void Save(AppState.PlayerSettings s)
    {
        try
        {
            var json = JsonSerializer.Serialize(s, _opts);
            using var f = FileAccess.Open(PATH, FileAccess.ModeFlags.Write);
            f.StoreString(json);
            GD.Print("[Settings] Saved");
        }
        catch (Exception e) { GD.PushWarning($"[Settings] Save failed: {e.Message}"); }
    }

    public static AppState.PlayerSettings LoadOrDefault()
    {
        try
        {
            if (!FileAccess.FileExists(PATH)) return new AppState.PlayerSettings();
            using var f = FileAccess.Open(PATH, FileAccess.ModeFlags.Read);
            var json = f.GetAsText();
            var s = JsonSerializer.Deserialize<AppState.PlayerSettings>(json);
            return s ?? new AppState.PlayerSettings();
        }
        catch (Exception e)
        {
            GD.PushWarning($"[Settings] Load failed: {e.Message}");
            return new AppState.PlayerSettings();
        }
    }
}