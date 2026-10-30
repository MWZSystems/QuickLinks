namespace QuickLinks.Models.Entities;

/// <summary>Parámetro de configuración editable por el administrador (CT_Settings).</summary>
public class Setting : AuditedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
