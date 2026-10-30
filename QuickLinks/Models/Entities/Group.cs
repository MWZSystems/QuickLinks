namespace QuickLinks.Models.Entities;

public class Group : AuditedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationGroup> Applications { get; set; } = [];
}
