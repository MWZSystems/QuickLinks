namespace QuickLinks.Models.Entities;

public class Application : AuditedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Version { get; set; }

    public string? Sme { get; set; }

    public string? UserManual { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    /// <summary>Nombre del archivo del logo dentro del almacenamiento de medios.</summary>
    public string? Image { get; set; }

    public ICollection<CategoryApplication> Categories { get; set; } = [];

    public ICollection<ApplicationGroup> Groups { get; set; } = [];

    public ICollection<ApplicationLocation> Locations { get; set; } = [];

    public ICollection<CarouselImage> CarouselImages { get; set; } = [];

    public ICollection<Maintenance> Maintenances { get; set; } = [];
}
