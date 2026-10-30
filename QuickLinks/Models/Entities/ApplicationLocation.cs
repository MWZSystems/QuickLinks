namespace QuickLinks.Models.Entities;

/// <summary>Relación aplicación-edificio (SY_ApplicationLocation).</summary>
public class ApplicationLocation : AuditedEntity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;

    public int BuildingId { get; set; }

    public Building Building { get; set; } = null!;
}
