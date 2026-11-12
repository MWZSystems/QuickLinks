using MWZSystem.Ui;

namespace QuickLinks.Services;

public sealed record CategoryCount(string Category, int Applications);

/// <summary>Conteos del portal para la página «Resumen» de administración.</summary>
public sealed record AdminCounts(int Applications, int ApplicationsInMaintenance, int ActiveAdvices, int ScheduledMaintenances, int Users, IReadOnlyList<CategoryCount> ByCategory);

/// <summary>Arma las piezas del resumen con conteos ya calculados.</summary>
public static class AdminSummary
{
    private const int MaximumCategories = 10;

    /// <param name="catalog">Dirección del catálogo de un controlador de administración.</param>
    public static IReadOnlyList<MwzStat> Stats(AdminCounts counts, Func<string, string> catalog) =>
    [
        new("Aplicaciones", counts.Applications.ToString("N0"), "window-stack", catalog("Applications")),
        new("En mantenimiento ahora", counts.ApplicationsInMaintenance.ToString("N0"), "tools", catalog("Maintenances"), counts.ApplicationsInMaintenance > 0 ? MwzTone.Aviso : MwzTone.Ok),
        new("Avisos activos", counts.ActiveAdvices.ToString("N0"), "megaphone", catalog("Advices")),
        new("Mantenimientos programados", counts.ScheduledMaintenances.ToString("N0"), "calendar-event", catalog("Maintenances")),
        new("Usuarios", counts.Users.ToString("N0"), "people", catalog("Users"))
    ];

    public static MwzBars ByCategory(AdminCounts counts)
    {
        var items = counts.ByCategory.OrderByDescending(x => x.Applications).ThenBy(x => x.Category, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new MwzBarItem(x.Category, x.Applications)).ToList();
        if (items.Count > MaximumCategories)
        {
            items = [.. items.Take(MaximumCategories - 1), new MwzBarItem("Otras", items.Skip(MaximumCategories - 1).Sum(x => x.Value))];
        }

        return new("Aplicaciones por categoría", items, EmptyText: "No hay aplicaciones asignadas a categorías.");
    }
}
