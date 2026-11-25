using System.Text.RegularExpressions;

namespace QuickLinks.Tests;

/// <summary>
/// Regla para todas las aplicaciones: en un formulario, una lista sin dato guardado aparece vacía («Selecciona...» sin
/// valor y el campo obligatorio), así nadie guarda el primer elemento sin elegirlo. Los filtros de búsqueda no cambian.
/// </summary>
public class FormSelectTests : IClassFixture<ActionColorsTests.App>
{
    private readonly ActionColorsTests.App _app;

    public FormSelectTests(ActionColorsTests.App app) => _app = app;

    [Fact]
    public void Cada_lista_de_formulario_tiene_la_opcion_vacia_y_es_obligatoria()
    {
        // Las listas de formulario son las que llevan asp-for; las del portal son filtros.
        var views = Directory.GetFiles(Path.Combine(LoadingScreenTests.ProjectFolder, "Views"), "*.cshtml", SearchOption.AllDirectories);
        var selects = views
            .SelectMany(v => Regex.Matches(File.ReadAllText(v), @"<select\b[^>]*asp-for[^>]*>(.*?)</select>", RegexOptions.Singleline)
                .Select(m => (View: Path.GetFileName(Path.GetDirectoryName(v)) + "/" + Path.GetFileName(v), Html: m.Value)))
            .ToList();

        Assert.Equal(3, selects.Count);
        Assert.All(selects, s =>
        {
            Assert.True(Regex.IsMatch(s.Html, @"<select\b[^>]*\brequired\b"), $"{s.View}: falta required");
            Assert.True(s.Html.Contains("<option value=\"\">Selecciona...</option>"), $"{s.View}: falta la opción vacía");
        });
    }

    [Theory]
    [InlineData("/Buildings/Create", "SiteId")]
    [InlineData("/Maintenances/Create", "ApplicationId")]
    [InlineData("/Users/Create", "RoleId")]
    public async Task En_un_alta_la_lista_aparece_sin_elegir(string page, string field)
    {
        var select = Select(await _app.GetAsync(page), field);
        // Ninguna opción marcada: el navegador muestra la primera, que es la vacía.
        Assert.Matches(@"<select\b[^>]*>\s*<option value="""">Selecciona...</option>", select);
        Assert.DoesNotContain("selected", select);
    }

    [Fact]
    public async Task En_una_edicion_la_lista_conserva_el_valor_guardado()
    {
        // El administrador inicial (Id 1) tiene el rol de administrador.
        var select = Select(await _app.GetAsync("/Users/Edit/1"), "RoleId");
        Assert.Matches(@"<option selected=""selected"" value=""1"">", select);
        Assert.DoesNotMatch(@"value="""" selected", select);
    }

    [Fact]
    public async Task El_servidor_no_supone_el_primer_elemento()
    {
        using var client = _app.CreateClient(new() { AllowAutoRedirect = false });
        var form = await client.GetStringAsync("/Buildings/Create");
        var token = Regex.Match(form, @"name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""").Groups[1].Value;

        using var response = await client.PostAsync("/Buildings/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token, ["SiteId"] = "", ["Name"] = "Edificio sin sitio", ["Available"] = "true",
        }));

        // Vuelve a mostrar el formulario con el error, sin guardar.
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Selecciona una opción.", System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    private static string Select(string html, string field) =>
        Regex.Match(html, $@"<select\b[^>]*name=""{field}""[^>]*>.*?</select>", RegexOptions.Singleline).Value;
}
