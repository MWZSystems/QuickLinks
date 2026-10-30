namespace QuickLinks.Models.ViewModels.Portal;

/// <summary>Opción de un filtro del portal. <see cref="ParentId"/> es el sitio de un edificio.</summary>
public sealed record FilterOption(int Id, string Name, int? ParentId = null);
