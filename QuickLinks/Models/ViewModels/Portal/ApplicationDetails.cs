namespace QuickLinks.Models.ViewModels.Portal;

/// <summary>Datos del modal de detalle de una aplicación.</summary>
public sealed record ApplicationDetails(
    int Id,
    string Name,
    string Description,
    string? Sme,
    string? UserManual,
    string? Version,
    string Link,
    string? Logo,
    IReadOnlyList<string> Images);
