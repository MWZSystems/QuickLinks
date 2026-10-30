using System.Text.RegularExpressions;

namespace QuickLinks.Infrastructure;

/// <summary>Utilidades de marca seguras para insertar en HTML y CSS.</summary>
public static partial class Branding
{
    public const string FallbackColor = "#0D4D84";

    /// <summary>Devuelve el color solo si es hexadecimal (#RGB o #RRGGBB); evita inyectar CSS arbitrario.</summary>
    public static string SafeColor(string? color) =>
        color is not null && HexColorRegex().IsMatch(color) ? color : FallbackColor;

    public static bool IsValidColor(string? color) => color is not null && HexColorRegex().IsMatch(color);

    [GeneratedRegex("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex HexColorRegex();
}
