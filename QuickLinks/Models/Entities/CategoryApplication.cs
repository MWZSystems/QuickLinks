namespace QuickLinks.Models.Entities;

/// <summary>Relación aplicación-categoría (CT_CategoryApplications).</summary>
public class CategoryApplication : AuditedEntity
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;
}
