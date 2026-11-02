using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
