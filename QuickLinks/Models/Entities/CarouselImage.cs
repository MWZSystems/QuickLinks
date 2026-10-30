namespace QuickLinks.Models.Entities;

/// <summary>Imagen del carrusel de detalle de una aplicación (CT_Carousel).</summary>
public class CarouselImage : AuditedEntity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;

    public string ImageName { get; set; } = string.Empty;
}
