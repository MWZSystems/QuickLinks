using System.Net;
using System.Text.RegularExpressions;
using MWZSystem.Ui;

namespace QuickLinks.Tests;

/// <summary>
/// Etiqueta de ambiente y versión de MWZSystem.Ui 1.8.0: todas las páginas dicen en qué ambiente corre el sitio y qué
/// versión de QuickLinks es («PRD · v1.0.0»), una sola vez y sin la versión escrita a mano de antes.
/// </summary>
public class EnvironmentLabelTests : IClassFixture<EnvironmentLabelTests.PrdApp>, IClassFixture<ActionColorsTests.App>
{
    private readonly PrdApp _prd;
    private readonly ActionColorsTests.App _dev;

    public EnvironmentLabelTests(PrdApp prd, ActionColorsTests.App dev) => (_prd, _dev) = (prd, dev);

    public static TheoryData<string> Pages() =>
        ["/", "/Account/Login", "/Summary", "/Applications", "/Applications/Edit/1", "/Settings/Brand", "/Home/Error"];

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task Cada_pagina_muestra_el_ambiente_y_la_version(string page)
    {
        var html = await _prd.GetAsync(page, anonymous: page is "/Account/Login" or "/Home/Error");

        var labels = Regex.Matches(html, @"<span class=""mwz-ambiente mwz-ambiente-prd""[^>]*data-mwz-ambiente=""PRD"">");
        Assert.Single(labels);
        // Razor codifica «·» y los acentos: se compara el texto ya decodificado.
        var text = WebUtility.HtmlDecode(html);
        Assert.Contains("<span aria-hidden=\"true\">PRD · v1.0.0</span>", text);
        Assert.Contains("Ambiente de producción (PRD), versión 1.0.0", text);
        // La versión ya no se repite junto al nombre de la aplicación.
        Assert.DoesNotContain("app-version", html);
    }

    [Fact]
    public async Task Sin_ambiente_definido_la_etiqueta_lo_dice()
    {
        // Este sitio de pruebas no define Mwz:Ambiente ni la variable ASPNETCORE_ENVIRONMENT: nunca se supone producción.
        // Por eso el sitio publicado lleva Mwz:Ambiente en su appsettings.Production.json.
        var text = WebUtility.HtmlDecode(await _dev.GetAsync("/Account/Login", anonymous: true));
        Assert.Contains("<span aria-hidden=\"true\">AMBIENTE SIN DEFINIR · v1.0.0</span>", text);
    }

    [Fact]
    public void La_version_mostrada_es_la_de_QuickLinks()
    {
        Assert.Equal("1.0.0", MwzEnvironment.VersionOf(typeof(Program).Assembly));
    }

    /// <summary>El sitio publicado: Mwz:Ambiente = PRD.</summary>
    public sealed class PrdApp : ActionColorsTests.App
    {
        protected override string? Ambiente => MwzEnvironment.Production;
    }
}
