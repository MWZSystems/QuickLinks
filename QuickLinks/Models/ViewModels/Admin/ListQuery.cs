namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Búsqueda, orden y página de un listado administrativo (query string).</summary>
public sealed class ListQuery
{
    public const int PageSize = 15;

    public string? Search { get; set; }

    public string? Sort { get; set; }

    public bool Desc { get; set; }

    public int Page { get; set; } = 1;

    /// <summary>Filtro opcional por sitio (aplicaciones y edificios).</summary>
    public int? SiteId { get; set; }

    /// <summary>Filtro opcional por edificio (aplicaciones).</summary>
    public int? BuildingId { get; set; }

    public string? Term => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

    /// <summary>Valores de ruta para generar un enlace con otro orden o página conservando los filtros.</summary>
    public Dictionary<string, string?> ToRoute(string? sort = null, bool? desc = null, int? page = null) => new()
    {
        ["Search"] = Search,
        ["Sort"] = sort ?? Sort,
        ["Desc"] = (desc ?? Desc) ? "true" : null,
        ["Page"] = (page ?? Page).ToString(),
        ["SiteId"] = SiteId?.ToString(),
        ["BuildingId"] = BuildingId?.ToString()
    };
}
