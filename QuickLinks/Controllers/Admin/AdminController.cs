using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Services;
using QuickLinks.Services.Auth;

namespace QuickLinks.Controllers.Admin;

/// <summary>Base de los catálogos administrativos: autorización, usuario actual y avisos.</summary>
[Authorize(Policy = AuthPolicies.Administrator)]
public abstract class AdminController : Controller
{
    protected int CurrentUserId => User.GetUserId();

    /// <summary>
    /// Avisa el resultado de activar o desactivar un registro. Si al desactivarlo algo deja de verse en el portal,
    /// el aviso es una advertencia que explica la consecuencia.
    /// </summary>
    protected void NotifyToggled(bool available, string activated, string deactivated, string? consequence = null)
    {
        if (available || consequence is null)
        {
            this.Success(available ? activated : deactivated);
        }
        else
        {
            this.Warning($"{deactivated} {consequence}");
        }
    }

    /// <summary>"1 aplicación ya no se muestra" o "3 aplicaciones ya no se muestran".</summary>
    protected static string AppsNoLongerShown(int count) =>
        count == 1 ? "1 aplicación ya no se muestra" : $"{count} aplicaciones ya no se muestran";

    /// <summary>Violación de índice único (2601) o de restricción UNIQUE (2627).</summary>
    protected static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
