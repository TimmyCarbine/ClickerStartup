using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using Label = Godot.Label;

public partial class Game : Control
{
    // === CONSTANTS ===
    private const string SAVE_PATH = "user://clicker_startup_save.json";

    // === STATE ===
    private AppState _app;
    private CurrencyManager _cm = new CurrencyManager();
    private UpgradeManager _um = new UpgradeManager();
    private readonly Dictionary<Button, Upgrade> _buttonToUpgrade = new();

    // === UI REFERENCES ===
    private VBoxContainer _upgradePanel;

    private Label _moneyLabel, _locLabel, _incomeLabel, _investorLabel;
    private TextureButton _settingsButton;
    private Button _writeCodeButton, _prestigeButton;
    private Button _buy1Button, _buy10Button, _buy100Button, _buyMaxButton;
    private ButtonGroup _buyGroup;
    private enum BuyMode { One = 1, Ten = 10, Hundred = 100, Max = -1 }
    private BuyMode _buyMode = BuyMode.One;
    private Timer _passiveTick, _autosave;

    private ConfirmationDialog _prestigeConfirmDialog;
    private AcceptDialog _prestigeSummaryDialog;
    private ProgressBar _prestigeProgressBar;



    public override void _Ready()
    {
        // Find nodes by their scene paths
        _upgradePanel = GetNode<VBoxContainer>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel");

        _moneyLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/MoneyLabel");
        _locLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/LocLabel");
        _incomeLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/IncomeLabel");
        _investorLabel = GetNode<Label>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/InvestorLabel");

        _writeCodeButton = GetNode<Button>("RootMargin/RootVBox/BodyHBox/ActionsRoot/ActionsPanel/WriteCodeButton");
        _prestigeButton = GetNode<Button>("RootMargin/RootVBox/PrestigeRoot/PrestigeHB/PrestigeButton");
        _settingsButton = GetNode<TextureButton>("RootMargin/RootVBox/HUDRoot/HUD/CurrencyHUD/SettingsButton");

        _buy1Button   = GetNode<Button>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel/UpgradesHeader/BuyModeBar/Buy1Button");
        _buy10Button  = GetNode<Button>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel/UpgradesHeader/BuyModeBar/Buy10Button");
        _buy100Button = GetNode<Button>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel/UpgradesHeader/BuyModeBar/Buy100Button");
        _buyMaxButton = GetNode<Button>("RootMargin/RootVBox/BodyHBox/UpgradesRoot/UpgradesScroll/UpgradesPanel/UpgradesHeader/BuyModeBar/BuyMaxButton");

        _buyGroup = new ButtonGroup();
        foreach (var b in new[] { _buy1Button, _buy10Button, _buy100Button, _buyMaxButton })
        {
            b.ToggleMode = true;
            b.ButtonGroup = _buyGroup;
        }

        _buy1Button.ButtonPressed = true;

        _passiveTick = GetNode<Timer>("PassiveTick");
        _autosave = GetNode<Timer>("Autosave");

        _prestigeConfirmDialog = GetNode<ConfirmationDialog>("PrestigeConfirmDialog");
        _prestigeSummaryDialog = GetNode<AcceptDialog>("PrestigeSummaryDialog");

        _prestigeProgressBar = GetNode<ProgressBar>("RootMargin/RootVBox/PrestigeRoot/PrestigeHB/PrestigeProgressRoot/PrestigeProgressBar");
        _prestigeProgressBar.MinValue = 0;
        _prestigeProgressBar.MaxValue = 1;

        // === APPSTATE CONNECTION ===
        _app = GetNode<AppState>("/root/AppState");

        // Apply persisted settings on game start
        var persisted = SettingsStorage.LoadOrDefault();
        ApplySettingsToGame(persisted);

        // Wire UI events
        _writeCodeButton.Pressed += OnWriteCodePressed;
        _settingsButton.Pressed += OpenSettings;

        _prestigeButton.Pressed += OnPrestigePressed;
        _prestigeConfirmDialog.Confirmed += OnPrestigeConfirmed;

        _buy1Button.Pressed   += () => { _buyMode = BuyMode.One;     RefreshHud(); };
        _buy10Button.Pressed  += () => { _buyMode = BuyMode.Ten;     RefreshHud(); };
        _buy100Button.Pressed += () => { _buyMode = BuyMode.Hundred; RefreshHud(); };
        _buyMaxButton.Pressed  += () => { _buyMode = BuyMode.Max;     RefreshHud(); };

        // Wire timers
        _passiveTick.Timeout += OnPassiveTick;
        _autosave.Timeout += OnAutosave;

        // Subscribe (use named handler, not lambda)
        _app.SettingsChanged += OnAppSettingsChanged;

        // Load upgrades
        var loadedUpgrades = _um.LoadUpgrades("res://Data/upgrades.json");
        BuildUpgradeUI();

        // Load save
        var loadedSaveData = SaveService.LoadAll(SAVE_PATH, _cm, _um);
        RefreshHud();

        var cmd = _app.ConsumeCommand();
        if (cmd == AppState.PendingCommand.ResetAll) PerformFullReset();

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
            if (!_um.IsUnlocked(u)) continue;

            var btn = new Button { CustomMinimumSize = new Vector2(260, 64) };
            UpdateUpgradeButton(btn, u);
            _buttonToUpgrade[btn] = u;

            btn.Pressed += () => OnUpgradePressed(u, btn);
            _upgradePanel.AddChild(btn);
        }
    }

    private void OnUpgradePressed(Upgrade u, Button btn)
    {
        int request = _buyMode switch
        {
            BuyMode.One     => 1,
            BuyMode.Ten     => 10,
            BuyMode.Hundred => 100,
            BuyMode.Max     => -1, // sentinel -> "max"
            _ => 1
        };

        if (_um.TryBuy(u, _cm, request, out var bought))
        {
            // Rebuild to reveal newly unlocked tiers & update costs
            BuildUpgradeUI();
            RefreshHud();
            SaveService.SaveAll(SAVE_PATH, _cm, _um);

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
        _cm.ApplyPassiveTick(1.0);
        RefreshHud();
    }

    private void OnPrestigePressed()
    {
        long preview = _cm.PreviewInvestorGain();
        string msg = preview > 0
            ? $"You will gain +{NumberFormatter.Format(preview)} Investor Capital" +
              $"(+{NumberFormatter.FormatPercent(preview * CurrencyManager.PCT_PER_IC)} global). \nProceed?"
            : $"You need to exceed your record (${NumberFormatter.Format(_cm.MaxMoneyEarned)}) by $10,000.\n" +
              $"Next target: ${NumberFormatter.Format(_cm.NextIcTargetMoney)}";
        _prestigeConfirmDialog.DialogText = msg;
        _prestigeConfirmDialog.Show();
    }

    private void OnPrestigeConfirmed()
    {
        long gained = _cm.DoPrestige();

        foreach (var u in _um.Upgrades) u.Purchases = 0;
        _cm.RebuildStatsFrom(_um.Upgrades);

        foreach (var child in _upgradePanel.GetChildren())
            if (child is Button) child.QueueFree();

        BuildUpgradeUI();
        RefreshHud();
        SaveService.SaveAll(SAVE_PATH, _cm, _um);

        _prestigeSummaryDialog.DialogText =
            $"Sold your startup! \nGained +{NumberFormatter.Format(gained)} Investor Capital. \n" +
            $"Global bonus is now (+{NumberFormatter.FormatPercent(_cm.GlobalBonusPercentPoints)})";
        _prestigeSummaryDialog.Show();

        GD.Print($"[Prestige] Gained {gained} capital. Total: {_cm.InvestorCapital} (Global Multiplier: {_cm.GlobalMult:0.00}x)");
    }

    private void OnAutosave() => SaveService.SaveAll(SAVE_PATH, _cm, _um);
    private string FormatUpgradeCount(Upgrade u) => u.Limit >= 0 ? $"{u.Purchases}/{u.Limit}" : $"{u.Purchases}/∞";
    private string FormatUpgradeCost(double cost) => $"${NumberFormatter.Format(cost)}";

    private void UpdateUpgradeButton(Button btn, Upgrade u)
    {
        int request = _buyMode switch
        {
            BuyMode.One => Math.Min(1, u.RemainingPurchases),
            BuyMode.Ten => Math.Min(10, u.RemainingPurchases),
            BuyMode.Hundred => Math.Min(100, u.RemainingPurchases),
            BuyMode.Max => u.GetMaxAffordable(_cm.Money),
            _ => 1
        };

        // If limited and out, bail early
        if (u.IsLimited && u.RemainingPurchases <= 0)
        {
            btn.Text = $"{u.Name} {FormatUpgradeCount(u)} (Max)";
            btn.Disabled = true;
            return;
        }

        // MAX UX: if request==0, show the next single price instead of a blank
        if (_buyMode == BuyMode.Max && request <= 0)
        {
            double nextCost = u.TotalCostFor(1); // same as u.CurrentCost
            btn.Text = $"{u.Name} {FormatUpgradeCount(u)}  x0 (MAX) — Next: {FormatUpgradeCost(nextCost)}";
            btn.Disabled = true;
            if (!string.IsNullOrEmpty(u.Description))
                btn.TooltipText = u.Description;
            return;
        }

        // Normal / MAX with request>0
        double total = u.TotalCostFor(request);
        bool canAfford = total > 0 && _cm.Money >= total;

        string qty = (_buyMode == BuyMode.Max) ? $"x{request} (MAX)" : $"x{request}";
        btn.Text = $"{u.Name} {FormatUpgradeCount(u)}  {qty} - {FormatUpgradeCost(total)}";
        btn.Disabled = !canAfford;

        if (!string.IsNullOrEmpty(u.Description))
            btn.TooltipText = u.Description;
    }

    private void RefreshHud()
    {
        _moneyLabel.Text = $"Money: {NumberFormatter.Format(_cm.Money)}";
        _locLabel.Text = $"LoC: {NumberFormatter.Format(_cm.LinesOfCode)}";
        _incomeLabel.Text = $"Income/sec: {NumberFormatter.Format(_cm.CurrentIncomePerSec)}";

        foreach (var (btn, u) in _buttonToUpgrade) UpdateUpgradeButton(btn, u);

        long preview = _cm.PreviewInvestorGain();

        // First IC target is always "record + 10k"
        double firstTarget = _cm.MaxMoneyEarned + 10_000.0;

        // Keep the bar pegged once you can earn IC
        double pct;
        if (preview > 0)
        {
            pct = 1.0;
        }
        else
        {
            // ⇨ Scale from 0 → firstTarget (ignore previous max in the ratio)
            pct = (firstTarget > 0 && !double.IsInfinity(firstTarget))
                ? Math.Clamp(_cm.Money / firstTarget, 0.0, 1.0)
                : 0.0;
        }
        _prestigeProgressBar.Value = pct;

        // For the "Next:" text you can still show the next milestone after you qualify
        double nextForText = (preview <= 0) ? firstTarget : _cm.NextIcTargetMoney;

        // Gate on eligibility (≥1 IC)
        bool canPrestige = preview > 0;
        _prestigeButton.Disabled = !canPrestige;

        _prestigeButton.Text = canPrestige
            ? $"Sell Company (+{NumberFormatter.Format(preview)} Investor Capital)"
            : $"Sell Company (Next: {NumberFormatter.Format(nextForText)})";

        _investorLabel.Text =
            $"IC: {NumberFormatter.Format(_cm.InvestorCapital)} " +
            $"(+{NumberFormatter.FormatPercent(_cm.GlobalBonusPercentPoints)})";
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) OpenSettings();
    }

    private void OpenSettings()
    {
        var app = GetNode<AppState>("/root/AppState");
        app.ReturnScenePath = "res://Scenes/Main.tscn";
        GetTree().ChangeSceneToFile("res://Scenes/Settings.tscn");
    }
    
    // === SETTINGS HANDLING ===
    private void OnAppSettingsChanged()
    {
        // Node may be exiting; guard before touching UI
        if (!IsHudAlive()) return;
        ApplySettingsToGame(_app.Settings);
    }

    public override void _ExitTree()
    {
        // Unsubscribe so we don't get signals after UI nodes are freed
        if (_app != null)
        {
            try { _app.SettingsChanged -= OnAppSettingsChanged; } catch { /* ignore */ }
        }
    }

    // Guard: ensures we don't write to freed labels
    private bool IsHudAlive()
    {
        return GodotObject.IsInstanceValid(_moneyLabel)
            && GodotObject.IsInstanceValid(_locLabel)
            && GodotObject.IsInstanceValid(_incomeLabel);
    }

    // Apply user settings dynamically
    private void ApplySettingsToGame(AppState.PlayerSettings st)
    {
        // number format
        NumberFormatter.CurrentMode =
            (st.NumberFormat == AppState.NumberFormatMode.Scientific)
            ? NumberFormatter.Mode.Scientific
            : NumberFormatter.Mode.Short;

        if (IsHudAlive()) RefreshHud();

        // autosave
        if (GodotObject.IsInstanceValid(_autosave))
        {
            _autosave.Stop();
            if (st.AutosaveEnabled)
            {
                _autosave.WaitTime = Math.Max(1.0, st.AutosaveIntervalSeconds);
                _autosave.OneShot = false;
                _autosave.Start(); // ← restarts with the new WaitTime
                GD.Print($"[Autosave] Enabled. WaitTime={_autosave.WaitTime:0.##}s");
            }
            else
            {
                GD.Print("[Autosave] Disabled.");
            }
        }
    }

    public void PerformFullReset()
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
        GD.Print("[Reset] Full reset performed");
    }

    private void OnTreeExiting()
    {
        SaveService.SaveAll(SAVE_PATH, _cm, _um);
    }
}
