using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QuickLinks.Services;

namespace QuickLinks.Tests;

public class FeedbackTests
{
    private sealed class MemoryProvider : ITempDataProvider
    {
        private IDictionary<string, object> values = new Dictionary<string, object>();
        public IDictionary<string, object> LoadTempData(HttpContext context) => values;
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => this.values = new Dictionary<string, object>(values);
    }

    private static TempDataDictionary NewTempData() => new(new DefaultHttpContext(), new MemoryProvider());

    [Fact]
    public void ErrorsComeFirstThenWarningsInfoAndSuccess()
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Success, "Categoría creada.");
        Feedback.Add(temp, FeedbackKind.Info, "Sin cambios.");
        Feedback.Add(temp, FeedbackKind.Error, "No se pudo guardar.");
        Feedback.Add(temp, FeedbackKind.Warning, "Categoría desactivada.");

        var kinds = Feedback.Take(temp).Select(m => m.Kind);

        Assert.Equal([FeedbackKind.Error, FeedbackKind.Warning, FeedbackKind.Info, FeedbackKind.Success], kinds);
    }

    [Fact]
    public void TheSameMessageIsKeptOnce()
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Success, "Sitio activado.");
        Feedback.Add(temp, FeedbackKind.Success, "Sitio activado.");

        Assert.Single(Feedback.Take(temp));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyTextsAreIgnored(string text)
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Info, text);

        Assert.Empty(Feedback.Take(temp));
    }

    [Fact]
    public void LegacyTempDataKeysAreShownWithoutDuplicates()
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Success, "Usuario creado.");
        temp["Success"] = "Usuario creado.";
        temp["Info"] = "Sin cambios.";
        temp["Warning"] = "Falta el logo.";
        temp["Error"] = "Otra persona cambió el registro.";

        var messages = Feedback.Take(temp);

        Assert.Equal(
            [
                new FeedbackMessage(FeedbackKind.Error, "Otra persona cambió el registro."),
                new FeedbackMessage(FeedbackKind.Warning, "Falta el logo."),
                new FeedbackMessage(FeedbackKind.Info, "Sin cambios."),
                new FeedbackMessage(FeedbackKind.Success, "Usuario creado.")
            ],
            messages);
    }

    [Fact]
    public void MessagesSurviveTheRedirectAndAreShownOnlyOnce()
    {
        var provider = new MemoryProvider();
        var post = new TempDataDictionary(new DefaultHttpContext(), provider);
        Feedback.Add(post, FeedbackKind.Success, "Grupo creado.");
        post.Save();

        var redirected = new TempDataDictionary(new DefaultHttpContext(), provider);
        Assert.Single(Feedback.Take(redirected));
        redirected.Save();

        var reloaded = new TempDataDictionary(new DefaultHttpContext(), provider);
        Assert.Empty(Feedback.Take(reloaded));
    }

    [Fact]
    public void NoticesKeepOrderAndKind()
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Success, "Categoría creada.");
        Feedback.Add(temp, FeedbackKind.Error, "No se pudo guardar.");

        var notices = Feedback.ToNotices(temp, new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());

        Assert.Equal([new(MWZSystem.Ui.MwzNoticeKind.Error, "No se pudo guardar."), new(MWZSystem.Ui.MwzNoticeKind.Success, "Categoría creada.")], notices);
    }

    [Fact]
    public void InvalidFormAddsGenericError()
    {
        var state = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        state.AddModelError("Name", "Escribe el nombre.");

        var notices = Feedback.ToNotices(NewTempData(), state);

        Assert.Equal([new(MWZSystem.Ui.MwzNoticeKind.Error, "Revisa los datos marcados en el formulario.")], notices);
    }

    [Fact]
    public void GeneralModelErrorsAreShownAsTheyAre()
    {
        var state = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        state.AddModelError("", "Usuario o contraseña incorrectos.");
        state.AddModelError("Name", "Escribe el nombre.");

        var notices = Feedback.ToNotices(NewTempData(), state);

        Assert.Equal([new(MWZSystem.Ui.MwzNoticeKind.Error, "Usuario o contraseña incorrectos.")], notices);
    }

    [Fact]
    public void InvalidFormDoesNotAddErrorWhenOneIsAlreadyPending()
    {
        var temp = NewTempData();
        Feedback.Add(temp, FeedbackKind.Error, "No se pudo guardar.");
        var state = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        state.AddModelError("Name", "Escribe el nombre.");

        Assert.Single(Feedback.ToNotices(temp, state));
    }

    [Fact]
    public void ErrorPageSkipsAntiforgeryCheck()
    {
        // La página de error se vuelve a ejecutar con el método original; si ese POST falló por el token vencido,
        // el filtro global no debe rechazar también la página de error.
        var action = typeof(QuickLinks.Controllers.HomeController).GetMethod("Error")!;

        Assert.NotEmpty(action.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute), false));
    }
}
