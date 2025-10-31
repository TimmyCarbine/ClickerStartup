using Godot;
using System;
using static AppState;

public partial class Settings : Control
{
    private OptionButton _formatOption;
    private CheckButton _autosaveCheck;
    private LineEdit _autosaveLine;
    private Button _applyBtn;
    private Button _backBtn;

    private AppState _app;
    private PlayerSettings _working;
    private bool _dirty;

    public override void _Ready()
    {
        _app = GetNode<AppState>("/root/AppState");

        _formatOption = GetNode<OptionButton>("MainVB/SettingsVB/NumberFormatHB/NumberFormatOptionButton");
        _autosaveCheck = GetNode<CheckButton>("MainVB/SettingsVB/AutosaveHB/AutosaveOptionCheckButton");
        _autosaveLine = GetNode<LineEdit>("MainVB/SettingsVB/AutosaveHB/AutosaveOptionLineEdit");
        _applyBtn = GetNode<Button>("MainVB/NavigationBar/ApplyButton");
        _backBtn = GetNode<Button>("MainVB/NavigationBar/BackButton");

        _formatOption.Clear();
        _formatOption.AddItem("Short", (int)NumberFormatMode.Short);
        _formatOption.AddItem("Scientific", (int)NumberFormatMode.Scientific);

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
        _applyBtn.Pressed += OnApplyPressed;
        _backBtn.Pressed += OnBackPressed;
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

    private async void OnBackPressed()
    {
        if (_dirty) GD.Print("[Settings] Unsaved changes discarded");

        var path = _app.ReturnScenePath;
        if (!string.IsNullOrEmpty(path))
            GetTree().ChangeSceneToFile(path);
        else
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
}
