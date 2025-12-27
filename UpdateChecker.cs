using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

public static class UpdateChecker
{
    // ✅ 改这里：你的 GitHub 用户名 + 仓库名
    private const string Owner = "Fansorigami";
    private const string Repo  = "RandomRollCall";

    private static readonly HttpClient _http = new HttpClient();

    public static async Task CheckForUpdatesAsync(IWin32Window ownerWindow)
    {
        try
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("RandomRollCall/1.0");

            var url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var latestTag = root.GetProperty("tag_name").GetString() ?? "";
            var htmlUrl   = root.GetProperty("html_url").GetString() ?? "";

            var current = NormalizeVersion(VersionInfo.GetAppVersion());
            var latest  = NormalizeVersion(latestTag);

            if (CompareSemVer(latest, current) > 0)
            {
                var r = MessageBox.Show(
                    ownerWindow,
                    $"发现新版本：{latestTag}\n当前版本：{VersionInfo.GetAppVersion()}\n\n要打开下载页面吗？",
                    "检查更新",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (r == DialogResult.Yes && !string.IsNullOrWhiteSpace(htmlUrl))
                    Process.Start(new ProcessStartInfo { FileName = htmlUrl, UseShellExecute = true });
            }
            else
            {
                MessageBox.Show(
                    ownerWindow,
                    $"你已经是最新版本啦！\n当前版本：{VersionInfo.GetAppVersion()}",
                    "检查更新",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ownerWindow, $"检查更新失败：{ex.Message}", "检查更新",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static (int major, int minor, int patch) NormalizeVersion(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return (0, 0, 0);
        v = v.Trim();
        if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase)) v = v[1..];

        var dash = v.IndexOf('-');
        if (dash >= 0) v = v[..dash];

        var parts = v.Split('.', StringSplitOptions.RemoveEmptyEntries);
        int major = parts.Length > 0 && int.TryParse(parts[0], out var a) ? a : 0;
        int minor = parts.Length > 1 && int.TryParse(parts[1], out var b) ? b : 0;
        int patch = parts.Length > 2 && int.TryParse(parts[2], out var c) ? c : 0;
        return (major, minor, patch);
    }

    private static int CompareSemVer((int major, int minor, int patch) a, (int major, int minor, int patch) b)
    {
        if (a.major != b.major) return a.major.CompareTo(b.major);
        if (a.minor != b.minor) return a.minor.CompareTo(b.minor);
        return a.patch.CompareTo(b.patch);
    }
}