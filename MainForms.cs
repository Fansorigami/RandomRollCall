using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

public sealed class MainForm : Form
{
    private const string AppName = "RandomRollCall";
    private readonly string _namesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "names.txt");

    private NotifyIcon _tray = null!;
    private ContextMenuStrip _menu = null!;

    private KeyboardHook _hook = null!;
    private RollCallService _roll = null!;
    private AppSettings _settings;

    private bool _exiting = false;

    public MainForm()
    {
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Visible = false;

        _settings = AppSettings.Load();

        _roll = new RollCallService(_namesPath);
        _roll.SetNoRepeat(_settings.NoRepeatMode);

        SetupTray();
        SetupHook();

        // —— 退出兜底（优化 4）——
        Application.ApplicationExit += (_, __) => Cleanup();
        AppDomain.CurrentDomain.ProcessExit += (_, __) => Cleanup();
        Application.ThreadException += (_, __) => Cleanup();
        AppDomain.CurrentDomain.UnhandledException += (_, __) => Cleanup();
    }

    private void SetupHook()
    {
        _hook = new KeyboardHook(_settings.TriggerKey);
        _hook.KeyPressed += _ =>
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(DoPick));
        };
        _hook.Start();
    }

    private void SetupTray()
    {
        _menu = new ContextMenuStrip();

        var itemPick = new ToolStripMenuItem("随机点名（或按触发键）", null, (_, __) => DoPick());
        var itemSettings = new ToolStripMenuItem("设置…", null, (_, __) => OpenSettings());
        var itemOpenNames = new ToolStripMenuItem("打开 names.txt", null, (_, __) => OpenNamesFile());
        var itemExit = new ToolStripMenuItem("退出", null, (_, __) => ExitApp());

        _menu.Items.Add(itemPick);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(itemSettings);
        _menu.Items.Add(itemOpenNames);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(itemExit);

        _tray = new NotifyIcon
        {
            Text = "随机点名",
            Visible = true,
            ContextMenuStrip = _menu,
            Icon = System.Drawing.SystemIcons.Information
        };

        _tray.DoubleClick += (_, __) => DoPick();
    }

private void DoPick()
{
    var name = _roll.Pick();
    var total = _roll.Total;

    string extra = _roll.NoRepeatMode
        ? $"本轮剩余：{_roll.RemainingInRound} / {total}"
        : $"允许重复，总人数：{total}";

    using var pop = new PopupForm(name, extra, _settings);
    pop.ShowDialog();
}

    private void OpenSettings()
    {
        using var dlg = new SettingsForm(
            _namesPath,
            _settings,
            AutoStart.IsEnabled(AppName)
        );

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _settings.Save();

            _roll.SetNoRepeat(_settings.NoRepeatMode);
            _roll.Reload();

            _hook.SetTargetKey(_settings.TriggerKey);

            AutoStart.Set(dlg.AutoStartEnabled, AppName);

            Toast("设置已保存");
        }
    }

    private void OpenNamesFile()
    {
        try
        {
            if (!File.Exists(_namesPath))
                File.WriteAllText(_namesPath, "张三\n李四\n王五\n");

            Process.Start(new ProcessStartInfo { FileName = _namesPath, UseShellExecute = true });
        }
        catch
        {
            Toast("打开失败：请检查权限");
        }
    }

    private void Toast(string text)
    {
        _tray.BalloonTipTitle = "随机点名";
        _tray.BalloonTipText = text;
        _tray.ShowBalloonTip(1200);
    }

    private void ExitApp()
    {
        _exiting = true;
        Cleanup();
        Application.Exit();
    }

    private void Cleanup()
    {
        if (_exiting == false)
        {
            // 非主动退出也要尽量保存
        }

        try { _settings.Save(); } catch { }

        try { _hook?.Dispose(); } catch { }

        try
        {
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
        }
        catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 点 X 只是隐藏常驻
        e.Cancel = true;
        Visible = false;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
    }
}