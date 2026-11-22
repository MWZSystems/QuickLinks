using System.Text.Json;
using System.Text.RegularExpressions;
using MWZSystem.Ui;

namespace QuickLinks.Tests;

/// <summary>
/// Pantalla de carga de MWZSystem.Ui (desde 1.6.0; la app usa 1.8.0): la app sirve esa versión, cada formulario que guarda dice qué está
/// haciendo, las descargas no dejan la página bloqueada y lo que se carga con fetch marca su parte como ocupada.
/// </summary>
public class LoadingScreenTests
{
    internal static readonly string ProjectFolder = FindProjectFolder();
    private static readonly string ViewsFolder = Path.Combine(ProjectFolder, "Views");

    [Fact]
    public void Usa_MWZSystem_Ui_1_8_0()
    {
        Assert.Equal(new Version(1, 8, 0, 0), typeof(MwzShell).Assembly.GetName().Version);
    }

    [Fact]
    public void La_app_sirve_el_mwz_ui_js_con_pantalla_de_carga()
    {
        var manifest = Path.Combine(AppContext.BaseDirectory, "QuickLinks.staticwebassets.runtime.json");
        using var json = JsonDocument.Parse(File.ReadAllText(manifest));
        var packageRoot = json.RootElement.GetProperty("ContentRoots").EnumerateArray()
            .Select(r => r.GetString()!)
            .Single(r => r.Contains("mwzsystem.ui", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(Path.Combine("mwzsystem.ui", "1.8.0"), packageRoot, StringComparison.OrdinalIgnoreCase);
        var script = File.ReadAllText(Path.Combine(packageRoot, "js", "mwz-ui.js"));
        Assert.Contains("mwz-texto-carga", script);
        Assert.Contains("mwz-descarga", script);
        Assert.Contains("ocupado", script);
    }

    [Fact]
    public void Cada_formulario_que_guarda_tiene_mensaje_de_carga()
    {
        var forms = Tags("form").Where(t => Regex.IsMatch(t.Tag, @"method=""post""", RegexOptions.IgnoreCase)).ToList();

        // Acceso, cerrar sesión, 8 formularios de administración, 6 de configuración y activar o desactivar.
        Assert.Equal(17, forms.Count);
        var missing = forms.Where(t => !t.Tag.Contains("data-mwz-texto-carga=\"")).Select(t => t.File).ToList();
        Assert.True(missing.Count == 0, "Sin data-mwz-texto-carga: " + string.Join(", ", missing));
        Assert.All(forms, t => Assert.DoesNotMatch(@"data-mwz-texto-carga=""\s*""", t.Tag));
    }

    [Fact]
    public void Las_descargas_no_bloquean_la_pagina()
    {
        var downloads = Tags("a").Concat(Tags("form"))
            .Where(t => Regex.IsMatch(t.Tag, @"\.(pptx|xlsx|pdf)\b"))
            .ToList();

        Assert.NotEmpty(downloads);
        Assert.All(downloads, t => Assert.Contains("data-mwz-descarga", t.Tag));
    }

    [Fact]
    public void La_busqueda_del_portal_no_muestra_la_carga_de_pagina_completa()
    {
        // Se resuelve con fetch sin cambiar de página: la capa general la dejaría puesta.
        var search = Tags("form").Single(t => t.Tag.Contains("id=\"portal-form\""));
        Assert.Contains("data-mwz-sin-carga", search.Tag);
    }

    [Fact]
    public void Lo_que_carga_el_portal_con_fetch_marca_su_parte_como_ocupada()
    {
        var script = File.ReadAllText(Path.Combine(ProjectFolder, "wwwroot", "js", "portal.js"));

        Assert.Equal(3, Regex.Matches(script, @"\bfetch\(").Count);
        Assert.Contains("mwz?.cargando(true, results)", script);
        Assert.Equal(2, Regex.Matches(script, @"mwz\?\.ocupado\(").Count);
    }

    /// <summary>Etiquetas de apertura de un elemento en todas las vistas (pueden ocupar varias líneas).</summary>
    private static IEnumerable<(string File, string Tag)> Tags(string element) =>
        Directory.EnumerateFiles(ViewsFolder, "*.cshtml", SearchOption.AllDirectories)
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), $@"<{element}\b[^>]*>", RegexOptions.Singleline)
                .Select(m => (Path.GetRelativePath(ViewsFolder, file), m.Value)));

    private static string FindProjectFolder()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var project = Path.Combine(dir.FullName, "QuickLinks", "QuickLinks.csproj");
            if (File.Exists(project))
            {
                return Path.GetDirectoryName(project)!;
            }
        }

        throw new DirectoryNotFoundException("No se encontró el proyecto QuickLinks.");
    }
}
