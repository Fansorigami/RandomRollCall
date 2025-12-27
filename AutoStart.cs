using Microsoft.Win32;
using System.Windows.Forms;

public static class AutoStart
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static void Set(bool enable, string appName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        if (key == null) return;

        if (enable)
            key.SetValue(appName, $"\"{Application.ExecutablePath}\"");
        else
            key.DeleteValue(appName, false);
    }

    public static bool IsEnabled(string appName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(appName) != null;
    }
}