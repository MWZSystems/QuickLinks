using MWZSystem.Ui;
using QuickLinks.Services;

namespace QuickLinks.Tests;

/// <summary>Resumen de administración: se arma solo con conteos ya calculados.</summary>
public class AdminSummaryTests
{
    private static AdminCounts Counts(int applications = 12, int inMaintenance = 0, int advices = 2, int scheduled = 1, int users = 3, params (string, int)[] categories) =>
        new(applications, inMaintenance, advices, scheduled, users, categories.Select(x => new CategoryCount(x.Item1, x.Item2)).ToList());

    [Fact]
    public void Los_indicadores_enlazan_a_su_catalogo()
    {
        var stats = AdminSummary.Stats(Counts(), controller => "/" + controller);
        Assert.Equal(["Aplicaciones", "En mantenimiento ahora", "Avisos activos", "Mantenimientos programados", "Usuarios"], stats.Select(x => x.Label));
        Assert.Equal(["/Applications", "/Maintenances", "/Advices", "/Maintenances", "/Users"], stats.Select(x => x.Url));
        Assert.Equal(["12", "0", "2", "1", "3"], stats.Select(x => x.Value));
    }

    [Fact]
    public void Mantenimiento_en_curso_se_marca_como_aviso()
    {
        Assert.Equal(MwzTone.Aviso, AdminSummary.Stats(Counts(inMaintenance: 2), c => c)[1].Tone);
        Assert.Equal(MwzTone.Ok, AdminSummary.Stats(Counts(inMaintenance: 0), c => c)[1].Tone);
    }

    [Fact]
    public void Sin_aplicaciones_la_grafica_queda_vacia()
    {
        Assert.True(AdminSummary.ByCategory(Counts()).IsEmpty);
    }

    [Fact]
    public void Las_categorias_se_ordenan_de_mayor_a_menor()
    {
        var bars = AdminSummary.ByCategory(Counts(categories: [("Calidad", 2), ("Producción", 7), ("Almacén", 2)]));
        Assert.Equal(["Producción", "Almacén", "Calidad"], bars.Items.Select(x => x.Label));
    }

    [Fact]
    public void Mas_de_diez_categorias_se_agrupan_en_Otras()
    {
        var bars = AdminSummary.ByCategory(Counts(categories: Enumerable.Range(1, 13).Select(i => ($"Categoría {i:00}", 14 - i)).ToArray()));
        Assert.Equal(10, bars.Items.Count);
        Assert.Equal("Otras", bars.Items[^1].Label);
        Assert.Equal(4 + 3 + 2 + 1, bars.Items[^1].Value);
        Assert.Equal(Enumerable.Range(1, 13).Sum(i => 14 - i), bars.Items.Sum(x => x.Value));
    }
}
