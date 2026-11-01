using System.ComponentModel.DataAnnotations;
using L = QuickLinks.Services.SettingLimits;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class SecuritySettingsForm
{
    [Range(L.MaxFailedMin, L.MaxFailedMax, ErrorMessage = "Entre {1} y {2} intentos.")]
    [Display(Name = "Intentos fallidos antes de bloquear")]
    public int MaxFailedAttempts { get; set; }

    [Range(L.LockoutMinutesMin, L.LockoutMinutesMax, ErrorMessage = "Entre {1} y {2} minutos.")]
    [Display(Name = "Minutos de bloqueo")]
    public int LockoutMinutes { get; set; }

    [Range(L.SessionMinutesMin, L.SessionMinutesMax, ErrorMessage = "Entre {1} y {2} minutos.")]
    [Display(Name = "Minutos de inactividad para cerrar la sesión")]
    public int SessionMinutes { get; set; }

    [Range(L.PasswordMinLengthMin, L.PasswordMinLengthMax, ErrorMessage = "Entre {1} y {2} caracteres.")]
    [Display(Name = "Longitud mínima de contraseña")]
    public int PasswordMinLength { get; set; }
}
