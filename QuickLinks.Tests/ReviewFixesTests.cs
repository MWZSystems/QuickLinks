using System.Net;
using System.Text.RegularExpressions;

namespace QuickLinks.Tests;

/// <summary>
/// Arreglos de la revisión con Lighthouse y MWZSystem.Ui 1.9.0: robots.txt sin sesión, tablas con nombre y encabezados de
/// columna, y descripción de la página.
/// </summary>
public class ReviewFixesTests : IClassFixture<ActionColorsTests.App>
{
    private readonly ActionColorsTests.App _app;

    public ReviewFixesTests(ActionColorsTests.App app) => _app = app;

    [Fact]
    public async Task Robots_txt_se_sirve_sin_sesion_y_no_deja_indexar()
    {
        // Antes redirigía (302) al inicio de sesión.
        var text = await _app.GetAsync("/robots.txt", anonymous: true);
        Assert.Contains("User-agent: *", text);
        Assert.Contains("Disallow: /", text);
    }

    public static TheoryData<string> Lists() =>
        ["/Applications", "/Categories", "/Groups", "/Sites", "/Buildings", "/Users", "/Advices", "/Maintenances"];

    [Theory]
    [MemberData(nameof(Lists))]
    public async Task Cada_tabla_tiene_caption_y_encabezados_de_columna(string page)
    {
        var html = await _app.GetAsync(page);
        Assert.Matches(@"<table[^>]*>\s*<caption class=""visually-hidden"">[^<]+</caption>", html);
        var headers = Regex.Matches(html, @"<th\b[^>]*>");
        Assert.NotEmpty(headers);
        Assert.All(headers, h => Assert.Contains(@"scope=""col""", h.Value));
    }

    [Theory]
    [InlineData("/Account/Login", true)]
    [InlineData("/", false)]
    [InlineData("/Applications/Create", false)]
    public async Task Las_paginas_cargan_los_grupos_de_MWZSystem_Ui_y_jQuery_una_vez(string page, bool anonymous)
    {
        // MWZSystem.Ui 1.10.0 junta CSS y JS para que el hosting no responda 503 con muchas peticiones.
        var html = await _app.GetAsync(page, anonymous);
        Assert.Single(Regex.Matches(html, @"<link[^>]*dist/mwz-base\.css"));
        Assert.Single(Regex.Matches(html, @"<script[^>]*dist/mwz-base\.js"));
        Assert.DoesNotMatch(@"<script[^>]*jquery(\.min)?\.js", html);
    }

    [Fact]
    public async Task El_portal_tiene_descripcion()
    {
        var html = WebUtility.HtmlDecode(await _app.GetAsync("/"));
        Assert.Matches(@"<meta name=""description"" content=""Accesos rápidos a las aplicaciones de [^""]+\.""", html);
    }
}
