namespace QuickLinks.Models.Entities;

/// <summary>Aviso general que aparece en la franja de noticias del portal (CT_Advices).</summary>
public class Advice : AuditedEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>Inicio de vigencia en UTC.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Fin de vigencia en UTC; nulo si no tiene fin.</summary>
    public DateTime? EndDate { get; set; }
}
