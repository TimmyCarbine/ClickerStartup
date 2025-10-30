using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Label = Godot.Label;

public partial class MainMenu : Control
{
    private Button _startButton, _settingsButton, _quitButton;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("Main/StartButton");
        _settingsButton = GetNode<Button>("Main/SettingsButton");
        _quitButton = GetNode<Button>("Main/QuitButton");

        _startButton.Pressed += OnStartPressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _quitButton.Pressed += OnQuitPressed;
    }

    private void OnStartPressed() => GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    private void OnSettingsPressed() => GetTree().ChangeSceneToFile("res://Scenes/Settings.tscn");
    private void OnQuitPressed() => GetTree().Quit();
}