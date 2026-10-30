namespace QuickLinks.Models.ViewModels.Portal;

/// <summary>Filtros del portal recibidos por query string.</summary>
public sealed class PortalFilter
{
    public string? Search { get; set; }

    public List<int> CategoryIds { get; set; } = [];

    public int? SiteId { get; set; }

    public int? BuildingId { get; set; }

    public int? GroupId { get; set; }

    public bool HasLocationOrGroup => SiteId.HasValue || BuildingId.HasValue || GroupId.HasValue;

    public bool HasAny => !string.IsNullOrWhiteSpace(Search) || CategoryIds.Count > 0 || HasLocationOrGroup;
}
