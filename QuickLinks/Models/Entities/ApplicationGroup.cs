namespace QuickLinks.Models.Entities;

/// <summary>Relación aplicación-grupo (SY_ApplicationGroups).</summary>
public class ApplicationGroup : AuditedEntity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;

    public int GroupId { get; set; }

    public Group Group { get; set; } = null!;
}
