using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Services;

namespace QuickLinks.Infrastructure;

/// <summary>
/// Un error inesperado al guardar (POST) no lleva a la página de error: se registra en el log y se vuelve a la
/// pantalla anterior con un aviso de error amable, sin detalles técnicos. Las consultas (GET) siguen usando la
/// página de error.
/// </summary>
public sealed class UiExceptionFilter(ITempDataDictionaryFactory tempDataFactory, ILogger<UiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var request = context.HttpContext.Request;
        if (!HttpMethods.IsPost(request.Method) || context.Exception is OperationCanceledException)
        {
            return;
        }

        logger.LogError(context.Exception, "Error al procesar {Method} {Path}", request.Method, request.Path);
        var message = context.Exception is DbUpdateConcurrencyException
            ? "Otra persona cambió este registro al mismo tiempo. Revisa los datos e intenta de nuevo."
            : "No se pudo completar la acción. Intenta de nuevo; si el problema sigue, avisa al administrador.";
        Feedback.Add(tempDataFactory.GetTempData(context.HttpContext), FeedbackKind.Error, message);

        var referer = request.Headers.Referer.ToString();
        var urlHelper = new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(context);
        context.Result = Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && string.Equals(uri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase)
            && urlHelper.IsLocalUrl(uri.PathAndQuery)
                ? new LocalRedirectResult(uri.PathAndQuery)
                : new RedirectResult(urlHelper.Content("~/"));
        context.ExceptionHandled = true;
    }
}
