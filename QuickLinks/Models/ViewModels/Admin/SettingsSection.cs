namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Apartado del menú de configuración: acción del controlador, título, icono y para qué sirve.</summary>
public sealed record SettingsSection(string Action, string Title, string Icon, string Description)
{
    public static readonly IReadOnlyList<SettingsSection> All =
    [
        new("Brand", "Marca", "bi-palette", "Nombre, empresa, color principal y logo del cliente."),
        new("Portal", "Portal", "bi-grid-3x3-gap", "Qué ve el usuario: buscador, filtros, contador, fijadas y recientes."),
        new("Notices", "Avisos", "bi-megaphone", "Etiqueta, color y velocidad de la franja de avisos."),
        new("Security", "Seguridad", "bi-shield-lock", "Bloqueo por intentos fallidos, duración de la sesión y contraseñas."),
        new("Media", "Imágenes", "bi-images", "Peso máximo de las imágenes y tamaño del carrusel."),
        new("Regional", "Regional", "bi-globe-americas", "Zona horaria y regla para validar enlaces.")
    ];
}
