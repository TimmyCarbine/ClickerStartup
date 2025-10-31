using Godot;
using System;

public partial class AppState : Node
{
    public string ReturnScenePath { get; set; } = "res://Scenes/MainMenu.tscn";
    public enum NumberFormatMode { Short, Scientific }
    public enum PendingCommand { None, ResetAll }
    public PendingCommand NextCommand { get; set; } = PendingCommand.None;

    public PendingCommand ConsumeCommand()
    {
        var c = NextCommand;
        NextCommand = PendingCommand.None;
        return c;
    }
    public class PlayerSettings
    {
        public NumberFormatMode NumberFormat { get; set; } = NumberFormatMode.Short;
        public bool AutosaveEnabled { get; set; } = true;
        public double AutosaveIntervalSeconds { get; set; } = 30.0;
    }

    public PlayerSettings Settings { get; private set; } = new();

    [Signal] public delegate void SettingsChangedEventHandler();

    public void ApplySettings(PlayerSettings s)
    {
        Settings = s;
        EmitSignal(SignalName.SettingsChanged);
    }
}