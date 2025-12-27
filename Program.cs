using System;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 线程异常（WinForms 常见）
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            try { MessageBox.Show(e.Exception.Message, "RandomRollCall crashed"); } catch { }
        };

        // 非 UI 线程异常
        AppDomain.CurrentDomain.UnhandledException += (_, __) => { /* 交给 MainForm 的双保险 */ };

        Application.Run(new MainForm());
    }
}