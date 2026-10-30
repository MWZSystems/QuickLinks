using System.Reflection;

namespace QuickLinks.Infrastructure;

public static class AppInfo
{
    /// <summary>Versión SemVer definida en el .csproj (sin el sufijo de commit).</summary>
    public static string Version { get; } =
        (typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0")
        .Split('+')[0];
}
