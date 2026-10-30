namespace QuickLinks.Data;

/// <summary>Valores fijos de la carga inicial incluida en las migraciones.</summary>
internal static class SeedData
{
    public const int AdminUserId = 1;

    /// <summary>Fecha fija para que HasData no genere cambios en cada migración.</summary>
    public static readonly DateTime Timestamp = new(2026, 10, 29, 0, 0, 0, DateTimeKind.Utc);
}
