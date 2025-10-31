using Godot;
using System;
using System.Threading.Tasks;
using static AppState;

public partial class Settings : Control
{
    private OptionButton _formatOption;
    private CheckButton _autosaveCheck;
    private LineEdit _autosaveLine;
    private Button _resetProgressBtn, _quitToMainBtn, _applyBtn, _backBtn;
    private ConfirmationDialog _resetConfirmDialog;
    private AppState _app;
    private PlayerSettings _working;
    private bool _dirty;

    public override void _Ready()
    {
        _app = GetNode<AppState>("/root/AppState");

        _formatOption = GetNode<OptionButton>("MainVB/SettingsVB/NumberFormatHB/NumberFormatOptionButton");
        _autosaveCheck = GetNode<CheckButton>("MainVB/SettingsVB/AutosaveHB/AutosaveOptionCheckButton");
        _autosaveLine = GetNode<LineEdit>("MainVB/SettingsVB/AutosaveHB/AutosaveOptionLineEdit");
        _resetProgressBtn = GetNode<Button>("MainVB/SettingsVB/ResetHB/ResetProgressButton");
        _quitToMainBtn = GetNode<Button>("MainVB/NavigationBar/QuitToMainButton");
        _applyBtn = GetNode<Button>("MainVB/NavigationBar/ApplyButton");
        _backBtn = GetNode<Button>("MainVB/NavigationBar/BackButton");

        _formatOption.Clear();
        _formatOption.AddItem("Short", (int)NumberFormatMode.Short);
        _formatOption.AddItem("Scientific", (int)NumberFormatMode.Scientific);

        _resetConfirmDialog = GetNode<ConfirmationDialog>("ResetConfirmDialog");

        var persisted = SettingsStorage.LoadOrDefault();
        _working = new PlayerSettings
        {
            NumberFormat = persisted.NumberFormat,
            AutosaveEnabled = persisted.AutosaveEnabled,
            AutosaveIntervalSeconds = persisted.AutosaveIntervalSeconds
        };

        _formatOption.Select((int)_working.NumberFormat);
        _autosaveCheck.ButtonPressed = _working.AutosaveEnabled;
        _autosaveLine.Text = Math.Clamp(_working.AutosaveIntervalSeconds, 1, 3600).ToString("0");
        _autosaveLine.Editable = _working.AutosaveEnabled;
        _applyBtn.Disabled = true;

        _formatOption.ItemSelected += idx => { _working.NumberFormat = (NumberFormatMode)idx; MarkDirty(); };
        _autosaveCheck.Toggled += on => { _working.AutosaveEnabled = on; _autosaveLine.Editable = on; MarkDirty(); };
        _autosaveLine.TextChanged += (string t) => { MarkDirty(); };
        _resetProgressBtn.Pressed += () => _resetConfirmDialog.Show();
        _quitToMainBtn.Pressed += () => GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");;
        _applyBtn.Pressed += OnApplyPressed;
        _backBtn.Pressed += OnBackPressed;

        _resetProgressBtn.Pressed += () => _resetConfirmDialog.Show();
        _resetConfirmDialog.Confirmed += OnResetConfirmed;

        _quitToMainBtn.Visible = _app.ReturnScenePath == "res://Scenes/Main.tscn";
    }

    private void MarkDirty()
    {
        _dirty = true;
        _applyBtn.Disabled = false;
    }

    private void OnApplyPressed()
    {
        if (!double.TryParse(_autosaveLine.Text, out var s) || s <= 0) s = 30;
        _working.AutosaveIntervalSeconds = Math.Clamp(s, 1, 3600);

        SettingsStorage.Save(_working);
        _app.ApplySettings(_working);

        _dirty = false;
        _applyBtn.Disabled = true;
        GD.Print("[Settings] Applied");
    }

    private void OnBackPressed()
    {
        if (_dirty) GD.Print("[Settings] Unsaved changes discarded");

        var path = _app.ReturnScenePath;
        GD.Print($"[Settings] Will return to {path}");
        if (string.IsNullOrEmpty(path)) path = "res://Scenes/Main.tscn";

        _app.ReturnScenePath = null;
        var tree = GetTree();
        if(tree != null) tree.ChangeSceneToFile(path);
    }

    private void OnResetConfirmed()
    {
        _app.NextCommand = AppState.PendingCommand.ResetAll;
        _app.ReturnScenePath = "res://Scenes/Main.tscn";

        var tree = GetTree();
        if(tree != null) tree.ChangeSceneToFile("res://Scenes/Main.tscn");
    }
}
