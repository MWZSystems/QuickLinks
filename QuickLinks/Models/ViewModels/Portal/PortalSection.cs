namespace QuickLinks.Models.ViewModels.Portal;

/// <summary>Bloque del modo completo: una categoría o el grupo seleccionado, con sus aplicaciones.</summary>
public sealed record PortalSection(string Title, string Color, IReadOnlyList<AppCard> Apps);
