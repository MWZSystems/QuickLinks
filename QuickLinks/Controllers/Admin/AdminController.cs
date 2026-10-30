using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Services.Auth;

namespace QuickLinks.Controllers.Admin;

/// <summary>Base de los catálogos administrativos: autorización, usuario actual y mensajes.</summary>
[Authorize(Policy = AuthPolicies.Administrator)]
public abstract class AdminController : Controller
{
    protected int CurrentUserId => User.GetUserId();

    protected void Notify(string message) => TempData["Success"] = message;

    /// <summary>Violación de índice único (2601) o de restricción UNIQUE (2627).</summary>
    protected static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
