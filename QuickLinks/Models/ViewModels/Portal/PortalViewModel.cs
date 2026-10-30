namespace QuickLinks.Models.ViewModels.Portal;

public sealed class PortalViewModel
{
    public required PortalFilter Filter { get; init; }

    public bool ShowAll { get; init; }

    /// <summary>Textos vigentes de avisos y mantenimientos que bloquean aplicaciones.</summary>
    public IReadOnlyList<string> Notices { get; init; } = [];

    public IReadOnlyList<AppCard> Pinned { get; init; } = [];

    public IReadOnlyList<AppCard> Recent { get; init; } = [];

    /// <summary>Resultados del modo simple: búsqueda global por nombre entre las aplicaciones activas.</summary>
    public IReadOnlyList<AppCard> SearchResults { get; init; } = [];

    public IReadOnlyList<PortalSection> Sections { get; init; } = [];

    public int AppCount { get; init; }

    public IReadOnlyList<FilterOption> Categories { get; init; } = [];

    public IReadOnlyList<FilterOption> Sites { get; init; } = [];

    public IReadOnlyList<FilterOption> Buildings { get; init; } = [];

    public IReadOnlyList<FilterOption> Groups { get; init; } = [];

    public bool IsSearching => !string.IsNullOrWhiteSpace(Filter.Search);
}
