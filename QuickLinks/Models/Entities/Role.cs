namespace QuickLinks.Models.Entities;

public class Role : AuditedEntity
{
    /// <summary>Identificador fijo del rol administrador; se crea en la migración inicial.</summary>
    public const int AdministratorId = 1;

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}
