namespace QuickLinks.Models.ViewModels.Portal;

/// <summary>Aplicación tal como se muestra en una tarjeta o botón del portal.</summary>
public sealed record AppCard(int Id, string Name, string Link, string? ImageUrl, bool IsBlocked);
