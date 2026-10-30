namespace QuickLinks.Models.Entities;

public class Category : AuditedEntity
{
    public const string DefaultColor = "#0D4D84";

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = DefaultColor;

    public ICollection<CategoryApplication> Applications { get; set; } = [];
}
