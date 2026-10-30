using QuickLinks.Models.ViewModels.Portal;

namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Catálogos disponibles para relacionar una aplicación. En edificios, ParentId es el sitio.</summary>
public sealed class ApplicationFormOptions
{
    public IReadOnlyList<FilterOption> Categories { get; init; } = [];

    public IReadOnlyList<FilterOption> Sites { get; init; } = [];

    public IReadOnlyList<FilterOption> Buildings { get; init; } = [];

    public IReadOnlyList<FilterOption> Groups { get; init; } = [];
}
