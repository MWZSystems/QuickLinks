namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Fila de avisos o mantenimientos. Las fechas están en UTC.</summary>
public sealed record ScheduleRow(int Id, string Title, string? Detail, DateTime StartDate, DateTime? EndDate, bool Flag, bool Available)
{
    public string Status
    {
        get
        {
            var now = DateTime.UtcNow;
            if (!Available)
            {
                return "Inactivo";
            }

            if (now <= StartDate)
            {
                return "Programado";
            }

            return EndDate is null || now < EndDate ? "Vigente" : "Vencido";
        }
    }
}
