using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class PopupForm : Form
{
    private readonly System.Windows.Forms.Timer _timer = new();

    public PopupForm(string text, AppSettings settings)
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var font = new Font("Segoe UI", settings.PopupFontSize, FontStyle.Bold);

        var label = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = font,
            Text = text
        };
        Controls.Add(label);

        // —— 自适配大小（测量文本 + padding）——
        // 给一个舒适的内边距
        int pad = 26;

        // 估算最大宽度：屏幕工作区的 55%
        var wa = Screen.PrimaryScreen!.WorkingArea;
        int maxW = (int)(wa.Width * 0.55);
        int minW = 320;

        // 先用一个宽度测量高度（模拟换行）
        var proposed = new Size(maxW - pad * 2, int.MaxValue);
        var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
        var measured = TextRenderer.MeasureText(text, font, proposed, flags);

        int w = Math.Clamp(measured.Width + pad * 2, minW, maxW);
        int h = Math.Clamp(measured.Height + pad * 2, 140, (int)(wa.Height * 0.35));

        Width = w;
        Height = h;

        // 右下角
        Left = wa.Right - Width - 20;
        Top = wa.Bottom - Height - 20;

        _timer.Interval = Math.Max(500, settings.PopupAutoCloseMs); // 固定 5s 也可以
        _timer.Tick += (_, __) => Close();
        _timer.Start();

        Click += (_, __) => Close();
        label.Click += (_, __) => Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        base.OnFormClosed(e);
    }
}