using System;
using System.Reflection;

public static class VersionInfo
{
    // 读取 "InformationalVersion"（可能是 v1.0.0-1-gxxxx）
    public static string GetInformationalVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? asm.GetName().Version?.ToString()
               ?? "unknown";
    }

    // 只取干净语义版本：1.2.3
    public static string GetCleanSemVer()
    {
        return NormalizeToSemVer(GetInformationalVersion());
    }

    // ✅ 给 UI 显示的版本：
    // Debug：显示完整（含 -gxxxx）
    // Release：只显示 v1.2.3
    public static string GetDisplayVersion()
    {
        var info = GetInformationalVersion();
#if DEBUG
        return info; // ✅ Debug 显示完整
#else
        var clean = GetCleanSemVer();
        return "v" + clean; // ✅ Release 显示干净
#endif
    }

    // 把 "v1.0.0-1-gabcd" / "1.0.0" / "1.0.0.0" 规整为 "1.0.0"
    private static string NormalizeToSemVer(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return "0.0.0";
        v = v.Trim();

        if (v.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            v = v.Substring(1);

        // 去掉 -xxxx 后缀
        var dash = v.IndexOf('-');
        if (dash >= 0) v = v.Substring(0, dash);

        // 只取前三段
        var parts = v.Split('.', StringSplitOptions.RemoveEmptyEntries);
        int major = parts.Length > 0 && int.TryParse(parts[0], out var a) ? a : 0;
        int minor = parts.Length > 1 && int.TryParse(parts[1], out var b) ? b : 0;
        int patch = parts.Length > 2 && int.TryParse(parts[2], out var c) ? c : 0;

        return $"{major}.{minor}.{patch}";
    }
}