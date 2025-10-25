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

    // === UI REFERENCES ===
    private Label _moneyLabel;
    private Label _locLabel;
    private Label _incomeLabel;

    private Button _writeCodeButton;
    private Button _buyClickPowerButton;

    private Timer _passiveTick;
    private Timer _autosave;



    public override void _Ready()
    {
        // Find nodes by their scene paths
        _moneyLabel = GetNode<Label>("HUD/CurrencyHUD/MoneyLabel");
        _locLabel = GetNode<Label>("HUD/CurrencyHUD/LocLabel");
        _incomeLabel = GetNode<Label>("HUD/CurrencyHUD/IncomeLabel");

        _writeCodeButton = GetNode<Button>("ActionsPanel/WriteCodeButton");
        _buyClickPowerButton = GetNode<Button>("ActionsPanel/BuyClickPowerButton");

        _passiveTick = GetNode<Timer>("PassiveTick");
        _autosave = GetNode<Timer>("Autosave");

        // Wire UI events
        _writeCodeButton.Pressed += OnWriteCodePressed;
        _buyClickPowerButton.Pressed += OnBuyClickPowerPressed;

        // Wire timers
        _passiveTick.Timeout += OnPassiveTick;
        _autosave.Timeout += OnAutosave;

        // Load save if present
        var loaded = SaveService.Load(SAVE_PATH);
        if (loaded != null)
        {
            _cm.ApplySave(loaded);
        }

        // Initial UI refresh
        RefreshHud();

        // Ensure autosave is at the interval we expect (optimal)
        _autosave.WaitTime = AUTOSAVE_INTERVAL_SEC;

        // Save on quit
        GetTree().TreeExiting += OnTreeExiting;
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
            _cm.ClickPower += 1;
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

    private void RefreshHud()
    {
        _moneyLabel.Text = $"Money: {NumberFormatter.Format(_cm.Money)}";
        _locLabel.Text = $"LoC: {NumberFormatter.Format(_cm.LinesOfCode)}";
        _incomeLabel.Text = $"Income/sec: {NumberFormatter.Format(_cm.IncomePerSecond)}";
    }
}
