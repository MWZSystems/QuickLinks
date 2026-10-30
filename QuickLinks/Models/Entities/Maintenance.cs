namespace QuickLinks.Models.Entities;

/// <summary>Ventana de mantenimiento de una aplicación (CT_Maintenance).</summary>
public class Maintenance : AuditedEntity
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public Application Application { get; set; } = null!;

    public string? CustomMessage { get; set; }

    /// <summary>Inicio de vigencia en UTC.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Fin de vigencia en UTC; nulo si no tiene fin.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Si es verdadero, el portal bloquea la apertura de la aplicación mientras esté vigente.</summary>
    public bool DisableApplication { get; set; }
}
