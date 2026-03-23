using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class PopupForm : Form
{
    private readonly System.Windows.Forms.Timer _timer = new();

    public PopupForm(string nameText, string extraText, AppSettings settings)
    {
        // —— 窗体基础属性 ——
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        // —— DPI 感知缩放系数 ——
        float dpiScale = DeviceDpi / 96f;

        // —— 字体层级（名字 / 说明） ——
        // 需求：名字字号扩大一倍
        var nameFont = new Font(
            "Segoe UI",
            settings.PopupFontSize * 2f,
            FontStyle.Bold,
            GraphicsUnit.Point
        );

        var extraFont = new Font(
            "Segoe UI",
            settings.PopupFontSize,   // 与名字比例保持原先 2:1
            FontStyle.Regular,
            GraphicsUnit.Point
        );

        // —— 标签：名字 ——
        var lblName = new Label
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = nameFont,
            Text = nameText,
            Dock = DockStyle.Top
        };

        // —— 标签：剩余说明 ——
        var lblExtra = new Label
        {
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = extraFont,
            ForeColor = Color.DimGray,
            Text = extraText,
            Dock = DockStyle.Top,
            Padding = new Padding(0, (int)(8 * dpiScale), 0, 0)
        };

        // —— 容器（避免挤在一起） ——
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(
                (int)(32 * dpiScale),
                (int)(24 * dpiScale),
                (int)(32 * dpiScale),
                (int)(24 * dpiScale)
            )
        };

        panel.Controls.Add(lblName);
        panel.Controls.Add(lblExtra);
        Controls.Add(panel);

        // —— 先让 WinForms 计算理想尺寸 ——
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        PerformLayout();

        // —— 需求：弹窗尺寸再扩大一倍 ——
        int targetWidth = Width * 2;
        int targetHeight = Height * 2;

        // —— 限制最大宽度（防止超宽） ——
        var wa = Screen.PrimaryScreen!.WorkingArea;
        int maxWidth = (int)(wa.Width * 0.7);
        Width = Math.Min(targetWidth, maxWidth);

        // 保证高度可控，不超出屏幕高度（留小空白）
        int maxHeight = (int)(wa.Height * 0.8);
        Height = Math.Min(targetHeight, maxHeight);

        // —— 位置：屏幕上半部分 + 水平居中 + 顶部留空 ——
        int topMargin = (int)(wa.Height * 0.12); // 距顶端约 12%
        Left = wa.Left + (wa.Width - Width) / 2;
        Top = wa.Top + topMargin;

        // —— 5 秒自动关闭 ——
        _timer.Interval = Math.Max(500, settings.PopupAutoCloseMs);
        _timer.Tick += (_, __) => Close();
        _timer.Start();

        Click += (_, __) => Close();
        lblName.Click += (_, __) => Close();
        lblExtra.Click += (_, __) => Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        base.OnFormClosed(e);
    }
}