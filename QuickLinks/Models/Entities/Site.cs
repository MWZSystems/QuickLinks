namespace QuickLinks.Models.Entities;

public class Site : AuditedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }

    public DateTime CreationDate { get; set; }

    public ICollection<Building> Buildings { get; set; } = [];
}
