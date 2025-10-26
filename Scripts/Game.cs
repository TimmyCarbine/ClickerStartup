using Godot;
using System;
using Label = Godot.Label;

public partial class Game : Control
{
    // === CONSTANTS ===
    private const string SAVE_PATH = "user://clicker-startup_save.json";
    private const int AUTOSAVE_INTERVAL_SEC = 30;

    // === STATE ===
    private CurrencyManager _cm = new CurrencyManager();
    private UpgradeManager _um = new UpgradeManager();

    // === UI REFERENCES ===
    private VBoxContainer _upgradePanel;

    private Label _moneyLabel;
    private Label _locLabel;
    private Label _incomeLabel;

    private Button _writeCodeButton;
    private Button _buyClickPowerButton;
    private Button _hireJuniorDevButton;
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
        _buyClickPowerButton = GetNode<Button>("RootMargin/RootVBox/BodyHBox/ActionsRoot/ActionsPanel/BuyClickPowerButton");
        _hireJuniorDevButton = GetNode<Button>("RootMargin/RootVBox/BodyHBox/ActionsRoot/ActionsPanel/HireJuniorDevButton");
        _resetProgressButton = GetNode<Button>("RootMargin/RootVBox/ResetRoot/ResetPanel/ResetProgressButton");

        _passiveTick = GetNode<Timer>("PassiveTick");
        _autosave = GetNode<Timer>("Autosave");

        _resetConfirmDialog = GetNode<ConfirmationDialog>("ResetConfirmDialog");

        // Wire UI events
        _writeCodeButton.Pressed += OnWriteCodePressed;
        _buyClickPowerButton.Pressed += OnBuyClickPowerPressed;
        _hireJuniorDevButton.Pressed += OnHireJuniorDevPressed;

        _resetProgressButton.Pressed += () => _resetConfirmDialog.Show();
        _resetConfirmDialog.Confirmed += OnResetConfirmed;

        // Wire timers
        _passiveTick.Timeout += OnPassiveTick;
        _autosave.Timeout += OnAutosave;

        // Load upgrades
        var loadedUpgrades = _um.LoadUpgrades("res://Data/upgrades.json");
        if (loadedUpgrades) BuildUpgradeUI();

        // Load save if present
        var loadedSaveData = SaveService.Load(SAVE_PATH);
        if (loadedSaveData != null)
        {
            _cm.ApplySave(loadedSaveData);
        }

        // Initial UI refresh
        RefreshHud();

        // Ensure autosave is at the interval we expect (optimal)
        _autosave.WaitTime = AUTOSAVE_INTERVAL_SEC;

        // Save on quit
        TreeExiting += OnTreeExiting;
    }

    private void BuildUpgradeUI()
    {
        foreach (var u in _um.Upgrades)
        {
            var btn = new Button
            {
                Text = $"{u.Name} - ${NumberFormatter.Format(u.Cost)}",
                TooltipText = u.Description
            };
            btn.Pressed += () => OnUpgradePressed(u, btn);
            _upgradePanel.AddChild(btn);
        }
    }

    private void OnUpgradePressed(Upgrade u, Button btn)
    {
        _um.ApplyUpgrade(u, _cm);
        RefreshHud();
        if (u.Purchased)
        {
            btn.Disabled = true;
            btn.Text = $"{u.Name} (Purchased)";
        }
    }

    private void OnWriteCodePressed()
    {
        _cm.AddOnClick();
        RefreshHud();
    }

    private void OnBuyClickPowerPressed()
    {
        const int COST = 10; // PLACEHOLDER
        if (_cm.TrySpend(COST))
        {
            _cm.AddClickPower(1);
            RefreshHud();
        }
    }

    private void OnHireJuniorDevPressed()
    {
        const int COST = 50; //PLACEHOLDER
        if (_cm.TrySpend(COST))
        {
            _cm.AddIncomePerSecond(1.0);
            RefreshHud();
        }
    }

    private void OnPassiveTick()
    {
        // Called once per second by the timer
        _cm.ApplyPassiveTick(1.0);
        RefreshHud();
    }

    private void OnAutosave()
    {
        SaveService.Save(SAVE_PATH, _cm);
    }

    private void OnTreeExiting()
    {
        SaveService.Save(SAVE_PATH, _cm);
    }

    private void OnResetConfirmed()
    {
        SaveService.Delete(SAVE_PATH);
        _cm.ResetAll();

        foreach (var u in _um.Upgrades)
            u.Purchased = false;

        foreach (var child in _upgradePanel.GetChildren())
            if(child is Button) child.QueueFree();

        BuildUpgradeUI();
        RefreshHud();

        SaveService.Save(SAVE_PATH, _cm);

        GD.Print("[Reset] Progress reset to defaults and save deleted.");
    }

    private void RefreshHud()
    {
        _moneyLabel.Text = $"Money: {NumberFormatter.Format(_cm.Money)}";
        _locLabel.Text = $"LoC: {NumberFormatter.Format(_cm.LinesOfCode)}";
        _incomeLabel.Text = $"Income/sec: {NumberFormatter.Format(_cm.IncomePerSecond)}";
    }
}
