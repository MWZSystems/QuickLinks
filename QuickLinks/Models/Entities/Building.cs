namespace QuickLinks.Models.Entities;

public class Building : AuditedEntity
{
    public int Id { get; set; }

    public int SiteId { get; set; }

    public Site Site { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationLocation> Applications { get; set; } = [];
}
