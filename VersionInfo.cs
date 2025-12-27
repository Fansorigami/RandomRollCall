using System.Reflection;

public static class VersionInfo
{
    public static string GetAppVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info)) return info;
        return asm.GetName().Version?.ToString() ?? "unknown";
    }
}