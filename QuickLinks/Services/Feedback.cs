using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MWZSystem.Ui;

namespace QuickLinks.Services;

/// <summary>Tipo de aviso emergente: éxito, información, advertencia o error.</summary>
public enum FeedbackKind
{
    Success, Info, Warning, Error
}

public record FeedbackMessage(FeedbackKind Kind, string Text);

/// <summary>
/// Avisos emergentes que se muestran en la siguiente pantalla, normalmente después de guardar y redirigir.
/// Uso en un controlador: <c>this.Success("Categoría creada.")</c>, <c>this.Info(...)</c>, <c>this.Warning(...)</c>
/// o <c>this.Error(...)</c>. En JavaScript: <c>QuickLinks.toast('success' | 'info' | 'warning' | 'error', texto)</c>.
/// La página los muestra con los avisos de MWZSystem.Ui (<see cref="ToNotices"/>).
/// </summary>
public static class Feedback
{
    private const string Key = "Feedback";
    // Claves anteriores (TempData["Success"], etc.): se siguen mostrando para no romper código que aún las use.
    private static readonly (string Key, FeedbackKind Kind)[] Legacy =
        [("Success", FeedbackKind.Success), ("Info", FeedbackKind.Info), ("Warning", FeedbackKind.Warning), ("Error", FeedbackKind.Error)];

    public static void Add(ITempDataDictionary tempData, FeedbackKind kind, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var list = Read(tempData.Peek(Key) as string);
        // Un mismo aviso no se repite (por ejemplo, dos errores iguales de una misma acción).
        if (!list.Contains(new(kind, text)))
        {
            list.Add(new(kind, text));
        }

        tempData[Key] = JsonSerializer.Serialize(list);
    }

    public static void Success(this Controller controller, string text) => Add(controller.TempData, FeedbackKind.Success, text);
    public static void Info(this Controller controller, string text) => Add(controller.TempData, FeedbackKind.Info, text);
    public static void Warning(this Controller controller, string text) => Add(controller.TempData, FeedbackKind.Warning, text);
    public static void Error(this Controller controller, string text) => Add(controller.TempData, FeedbackKind.Error, text);

    /// <summary>Lee y consume los avisos pendientes; los errores van primero.</summary>
    public static List<FeedbackMessage> Take(ITempDataDictionary tempData)
    {
        var list = Read(tempData[Key] as string);
        foreach (var (key, kind) in Legacy)
        {
            if (tempData[key] is string text && !string.IsNullOrWhiteSpace(text) && !list.Contains(new(kind, text)))
            {
                list.Add(new(kind, text));
            }
        }

        return list.OrderByDescending(m => m.Kind).ToList();
    }

    /// <summary>
    /// Avisos de la página en el formato de MWZSystem.Ui. Un formulario devuelto con datos inválidos también avisa:
    /// los errores generales (sin campo, como «usuario o contraseña incorrectos») se muestran tal cual; si solo hay
    /// errores de campo se pide revisar el formulario, porque el detalle ya está junto a cada campo.
    /// </summary>
    public static List<MwzNotice> ToNotices(ITempDataDictionary tempData, ModelStateDictionary modelState)
    {
        var messages = Take(tempData);
        if (!modelState.IsValid && messages.All(m => m.Kind != FeedbackKind.Error))
        {
            var general = modelState.Where(e => e.Key == "").SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage).Where(t => t != "").Distinct().ToList();
            messages.InsertRange(0, general.Count > 0
                ? general.Select(t => new FeedbackMessage(FeedbackKind.Error, t))
                : [new(FeedbackKind.Error, "Revisa los datos marcados en el formulario.")]);
        }

        return messages.Select(m => new MwzNotice(m.Kind switch
        {
            FeedbackKind.Success => MwzNoticeKind.Success,
            FeedbackKind.Info => MwzNoticeKind.Info,
            FeedbackKind.Warning => MwzNoticeKind.Warning,
            _ => MwzNoticeKind.Error
        }, m.Text)).ToList();
    }

    private static List<FeedbackMessage> Read(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<FeedbackMessage>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
