using Godot;

public partial class MainMenu : Control
{
    private Button _startButton, _settingsButton, _quitButton;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("Main/StartButton");
        _settingsButton = GetNode<Button>("Main/SettingsButton");
        _quitButton = GetNode<Button>("Main/QuitButton");

        _startButton.Pressed += () => GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        _settingsButton.Pressed += OpenSettings;
        _quitButton.Pressed += () => GetTree().Quit();
    }
    private void OpenSettings()
    {
        var app = GetNode<AppState>("/root/AppState");
        app.ReturnScenePath = "res://Scenes/MainMenu.tscn";
        GetTree().ChangeSceneToFile("res://Scenes/Settings.tscn");
    }
}