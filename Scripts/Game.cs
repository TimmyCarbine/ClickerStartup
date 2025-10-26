using Godot;
using System;
using System.Collections.Generic;
using Label = Godot.Label;

public partial class Game : Control
{
    // === CONSTANTS ===
    private const string SAVE_PATH = "user://clicker_startup_save.json";
    private const int AUTOSAVE_INTERVAL_SEC = 30;

    // === STATE ===
    private CurrencyManager _cm = new CurrencyManager();
    private UpgradeManager _um = new UpgradeManager();
    private readonly Dictionary<Button, Upgrade> _buttonToUpgrade = new();

    // === UI REFERENCES ===
    private VBoxContainer _upgradePanel;

    private Label _moneyLabel;
    private Label _locLabel;
    private Label _incomeLabel;

    private Button _writeCodeButton;
    private Button _resetProgressButton;

    private Timer _passiveTick;
    private Timer _autosave;

    private ConfirmationDialog _resetConfirmDialog;



    public override void _Ready()
    {
        // Find nodes by their scene paths
        _upgradePanel = GetNode<VBoxContainer>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel");

        _moneyLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/MoneyLabel");
        _locLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/LocLabel");
        _incomeLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/IncomeLabel");

        _writeCodeButton = GetNode<Button>("RootMargin/RootVBox/BodyHBox/ActionsRoot/ActionsPanel/WriteCodeButton");
        _resetProgressButton = GetNode<Button>("RootMargin/RootVBox/ResetRoot/ResetPanel/ResetProgressButton");

        _passiveTick = GetNode<Timer>("PassiveTick");
        _autosave = GetNode<Timer>("Autosave");

        _resetConfirmDialog = GetNode<ConfirmationDialog>("ResetConfirmDialog");

        // Wire UI events
        _writeCodeButton.Pressed += OnWriteCodePressed;

        _resetProgressButton.Pressed += () => _resetConfirmDialog.Show();
        _resetConfirmDialog.Confirmed += OnResetConfirmed;

        // Wire timers
        _passiveTick.Timeout += OnPassiveTick;
        _autosave.Timeout += OnAutosave;

        // Load upgrades
        var loadedUpgrades = _um.LoadUpgrades("res://Data/upgrades.json");
        BuildUpgradeUI();

        // Load save
        var loadedSaveData = SaveService.LoadAll(SAVE_PATH, _cm, _um);
        RefreshHud();

        // Ensure autosave is at the interval we expect (optimal)
        _autosave.WaitTime = AUTOSAVE_INTERVAL_SEC;

        // Save on quit
        TreeExiting += OnTreeExiting;
    }

    private void BuildUpgradeUI()
    {
        foreach (var child in _upgradePanel.GetChildren())
            if (child is Button) child.QueueFree();

        _buttonToUpgrade.Clear();

        foreach (var u in _um.Upgrades)
        {
            var btn = new Button { CustomMinimumSize = new Vector2(260, 56) };
            UpdateUpgradeButton(btn, u);
            _buttonToUpgrade[btn] = u;

            btn.Pressed += () => OnUpgradePressed(u, btn);
            _upgradePanel.AddChild(btn);
        }
    }

    private void OnUpgradePressed(Upgrade u, Button btn)
    {
        if (_um.TryBuy(u, _cm))
        {
            RefreshHud();
            UpdateUpgradeButton(btn, u);

            // Optional: Purchase Sound / Save
        }
        else
        {
            // Optional: Fail Sound / Message
        }
    }

    private void OnWriteCodePressed()
    {
        _cm.AddOnClick();
        RefreshHud();
    }

    private void OnPassiveTick()
    {
        // Called once per second by the timer
        _cm.ApplyPassiveTick(1.0);
        RefreshHud();
        GD.Print($"Click Flat: {_cm.ClickFlat}"); // PLACEHOLDER
        GD.Print($"Click Mult: {_cm.ClickMult}"); // PLACEHOLDER
        GD.Print($"Income Flat: {_cm.IncomeFlat}"); // PLACEHOLDER
        GD.Print($"Income Mult: {_cm.IncomeMult}"); // PLACEHOLDER
    }

    private void OnAutosave()
    {
        SaveService.SaveAll(SAVE_PATH, _cm, _um);
    }

    private void OnTreeExiting()
    {
        SaveService.SaveAll(SAVE_PATH, _cm, _um);
    }

    private void OnResetConfirmed()
    {
        SaveService.Delete(SAVE_PATH);
        _cm.ResetAll();

        foreach (var u in _um.Upgrades) u.Purchases = 0;

        _um.ReapplyAll(_cm);
        foreach (var child in _upgradePanel.GetChildren())
            if (child is Button) child.QueueFree();

        BuildUpgradeUI();
        RefreshHud();

        SaveService.SaveAll(SAVE_PATH, _cm, _um);

        GD.Print("[Reset] Progress reset to defaults and save deleted.");
    }

    private string FormatUpgradeCount(Upgrade u)
    {
        return u.Limit >= 0 ? $"{u.Purchases}/{u.Limit}" : $"{u.Purchases}/∞";
    }

    private string FormatUpgradeCost(double cost)
    {
        return $"${NumberFormatter.Format(cost)}";
    }

    private void UpdateUpgradeButton(Button btn, Upgrade u)
    {
        var canAfford = _cm.Money >= u.CurrentCost;

        if (u.IsLimited)
        {
            btn.Text = $"{u.Name} {FormatUpgradeCount(u)} (Max)";
            btn.Disabled = true;
        }
        else
        {
            btn.Text = $"{u.Name} {FormatUpgradeCount(u)} - {FormatUpgradeCost(u.CurrentCost)}";
            btn.Disabled = !canAfford;
        }

        if (!string.IsNullOrEmpty(u.Description))
            btn.TooltipText = u.Description;
    }

    private void RefreshHud()
    {
        _moneyLabel.Text = $"Money: {NumberFormatter.Format(_cm.Money)}";
        _locLabel.Text = $"LoC: {NumberFormatter.Format(_cm.LinesOfCode)}";
        _incomeLabel.Text = $"Income/sec: {NumberFormatter.Format(_cm.CurrentIncomePerSec)}";

        foreach (var (btn, u) in _buttonToUpgrade)
            UpdateUpgradeButton(btn, u);
    }
}
