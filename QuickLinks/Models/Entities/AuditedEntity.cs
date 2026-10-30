namespace QuickLinks.Models.Entities;

/// <summary>
/// Columnas de auditoría y baja lógica comunes a los catálogos (FKUserUpdater, LastUpdated, Available).
/// </summary>
public abstract class AuditedEntity
{
    public int UpdatedByUserId { get; set; }

    public DateTime LastUpdated { get; set; }

    public bool Available { get; set; } = true;
}
