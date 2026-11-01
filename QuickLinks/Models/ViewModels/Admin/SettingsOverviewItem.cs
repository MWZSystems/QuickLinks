namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Tarjeta del menú de configuración con el resumen de los valores actuales del apartado.</summary>
public sealed record SettingsOverviewItem(SettingsSection Section, IReadOnlyList<(string Label, string Value)> Summary);
