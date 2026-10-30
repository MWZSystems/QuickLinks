namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Fila genérica de listado: nombre, dato secundario, conteo de aplicaciones y estado.</summary>
public sealed record NamedRow(int Id, string Name, string? Detail, int ApplicationCount, DateTime LastUpdated, bool Available);
