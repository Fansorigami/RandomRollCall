using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public sealed class SettingsForm : Form
{
    public bool AutoStartEnabled => _chkAutoStart.Checked;

    private readonly string _namesPath;
    private readonly AppSettings _settings;

    private readonly Label _hint = new();
    private readonly TextBox _txtNames = new();
    private readonly Label _lblKey = new();
    private readonly Button _btnCaptureKey = new();
    private readonly CheckBox _chkNoRepeat = new();
    private readonly CheckBox _chkAutoStart = new();
    private readonly NumericUpDown _numFontSize = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnCancel = new();

    private bool _capturing = false;

    public SettingsForm(string namesPath, AppSettings settings, bool autoStart)
    {
        _namesPath = namesPath;
        _settings = settings;

        Text = "设置";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 640;
        Height = 600;
        Font = new Font("Segoe UI", 10);
        KeyPreview = true;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 1,
            Padding = new Padding(12),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // hint
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // title
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // names
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // key row
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // no repeat
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); // font
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // autostart
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // buttons
        Controls.Add(root);

        _hint.Dock = DockStyle.Fill;
        _hint.Text = "提示：名单每行一个名字；触发键建议用 F8 / Pause / ScrollLock。";
        _hint.ForeColor = Color.DimGray;
        root.Controls.Add(_hint, 0, 0);

        root.Controls.Add(new Label { Text = "名单（每行一个名字）", Dock = DockStyle.Fill }, 0, 1);

        _txtNames.Multiline = true;
        _txtNames.ScrollBars = ScrollBars.Vertical;
        _txtNames.Dock = DockStyle.Fill;
        _txtNames.Text = File.Exists(_namesPath) ? File.ReadAllText(_namesPath) : "";
        root.Controls.Add(_txtNames, 0, 2);

        // 触发键行
        var keyRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        keyRow.Controls.Add(new Label { Text = "触发键：", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });

        _lblKey.Text = _settings.TriggerKey.ToString();
        _lblKey.AutoSize = true;
        _lblKey.Padding = new Padding(0, 8, 10, 0);
        keyRow.Controls.Add(_lblKey);

        _btnCaptureKey.Text = "修改触发键";
        _btnCaptureKey.AutoSize = true;
        _btnCaptureKey.Click += (_, __) => EnterCaptureMode();
        keyRow.Controls.Add(_btnCaptureKey);

        root.Controls.Add(keyRow, 0, 3);

        _chkNoRepeat.Text = "去重模式（本轮不重复，抽完自动重置）";
        _chkNoRepeat.Checked = _settings.NoRepeatMode;
        _chkNoRepeat.Dock = DockStyle.Fill;
        root.Controls.Add(_chkNoRepeat, 0, 4);

        // 字体大小
        var fontRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        fontRow.Controls.Add(new Label { Text = "弹窗字体大小：", AutoSize = true, Padding = new Padding(0, 10, 0, 0) });

        _numFontSize.Minimum = 10;
        _numFontSize.Maximum = 48;
        _numFontSize.DecimalPlaces = 0;
        _numFontSize.Value = (decimal)_settings.PopupFontSize;
        _numFontSize.Width = 90;

        fontRow.Controls.Add(_numFontSize);
        fontRow.Controls.Add(new Label { Text = "pt", AutoSize = true, Padding = new Padding(6, 10, 0, 0) });
        root.Controls.Add(fontRow, 0, 5);

        _chkAutoStart.Text = "开机自启（当前用户）";
        _chkAutoStart.Checked = autoStart;
        _chkAutoStart.Dock = DockStyle.Fill;
        root.Controls.Add(_chkAutoStart, 0, 6);

        var btnRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };

        _btnSave.Text = "保存";
        _btnSave.Width = 110;
        _btnSave.Click += (_, __) => SaveAndClose();

        _btnCancel.Text = "取消";
        _btnCancel.Width = 110;
        _btnCancel.Click += (_, __) => { DialogResult = DialogResult.Cancel; Close(); };

        btnRow.Controls.Add(_btnSave);
        btnRow.Controls.Add(_btnCancel);
        root.Controls.Add(btnRow, 0, 7);
    }

    private void EnterCaptureMode()
    {
        _capturing = true;
        _hint.Text = "正在捕获按键：请按下新的触发键（按 Esc 取消）。";
        _hint.ForeColor = Color.Firebrick;

        // 禁用其他控件，避免误操作（优化 2）
        _txtNames.Enabled = false;
        _chkNoRepeat.Enabled = false;
        _chkAutoStart.Enabled = false;
        _numFontSize.Enabled = false;
        _btnSave.Enabled = false;
        _btnCancel.Enabled = false;

        _btnCaptureKey.Text = "捕获中…（Esc 取消）";
        ActiveControl = null;
    }

    private void ExitCaptureMode(bool canceled)
    {
        _capturing = false;
        _hint.Text = canceled
            ? "已取消捕获。提示：触发键建议用 F8 / Pause / ScrollLock。"
            : "提示：名单每行一个名字；触发键建议用 F8 / Pause / ScrollLock。";
        _hint.ForeColor = Color.DimGray;

        _txtNames.Enabled = true;
        _chkNoRepeat.Enabled = true;
        _chkAutoStart.Enabled = true;
        _numFontSize.Enabled = true;
        _btnSave.Enabled = true;
        _btnCancel.Enabled = true;

        _btnCaptureKey.Text = "修改触发键";
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_capturing)
        {
            if (keyData == Keys.Escape)
            {
                ExitCaptureMode(canceled: true);
                return true;
            }

            _settings.TriggerKey = keyData;
            _lblKey.Text = keyData.ToString();
            ExitCaptureMode(canceled: false);
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SaveAndClose()
    {
        // 空名单提示（避免上课翻车）
        var content = _txtNames.Text.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            MessageBox.Show("名单不能为空哦～至少写一个名字（每行一个）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        File.WriteAllText(_namesPath, content);

        _settings.NoRepeatMode = _chkNoRepeat.Checked;
        _settings.PopupFontSize = (float)_numFontSize.Value;
        _settings.PopupAutoCloseMs = 5000; // 固定 5s（你要求）

        DialogResult = DialogResult.OK;
        Close();
    }
}