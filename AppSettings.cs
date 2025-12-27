using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public sealed class AppSettings
{
    public float PopupFontSize { get; set; } = 18f;
    public int PopupAutoCloseMs { get; set; } = 5000;
    public bool NoRepeatMode { get; set; } = true;
    public Keys TriggerKey { get; set; } = Keys.F8;

    public static string SettingsDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RandomRollCall");

    public static string SettingsPath => Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}