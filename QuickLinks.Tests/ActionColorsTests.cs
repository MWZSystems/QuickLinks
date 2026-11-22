using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MWZSystem.Ui;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Services.Auth;

namespace QuickLinks.Tests;

/// <summary>
/// Colores por acción de MWZSystem.Ui 1.7.0 (criterio de Trainify): cada pantalla principal, ya dibujada, tiene sus botones
/// con el color de lo que hacen. Las páginas corren contra una base SQLite en memoria con datos de prueba y un administrador.
/// </summary>
public class ActionColorsTests : IClassFixture<ActionColorsTests.App>
{
    /// <summary>El administrador inicial (SeedData.AdminUserId).</summary>
    private const int AdminId = 1;

    private readonly App _app;

    public ActionColorsTests(App app) => _app = app;

    public static TheoryData<string> Screens() =>
    [
        "/", "/Account/Login", "/Summary",
        "/Applications", "/Applications/Create", "/Applications/Edit/1",
        "/Categories", "/Categories/Create", "/Groups", "/Sites", "/Buildings", "/Users", "/Advices", "/Maintenances",
        "/Settings", "/Settings/Brand", "/Settings/Portal", "/Settings/Security",
    ];

    [Theory]
    [MemberData(nameof(Screens))]
    public async Task Cada_pantalla_tiene_sus_botones_con_el_color_de_su_accion(string screen)
    {
        var html = await _app.GetAsync(screen, anonymous: screen == "/Account/Login");
        Assert.Empty(MwzActionColors.Problems(html));
    }

    [Fact]
    public async Task Las_paginas_activan_los_colores_y_desactivar_sale_en_rojo()
    {
        Assert.Contains("mwz-colores-accion", await _app.GetAsync("/"));
        Assert.Contains("mwz-colores-accion", await _app.GetAsync("/Account/Login", anonymous: true));

        // Botones de la lista: el texto se oculta en pantallas angostas, así que dicen su acción con data-mwz-accion.
        var list = await _app.GetAsync("/Applications");
        Assert.Contains("mwz-colores-accion", list);
        Assert.Matches(Button("btn btn-sm btn-warning", "editar"), list);
        Assert.Matches(Button("btn btn-sm btn-danger", "desactivar"), list);
        Assert.Matches(Button("btn btn-sm btn-success", "activar"), await _app.GetAsync("/Categories"));
    }

    [Fact]
    public async Task Botones_con_texto_cambian_al_color_de_su_accion()
    {
        var form = await _app.GetAsync("/Applications/Create");
        Assert.Matches(@"class=""btn px-4 btn-primary""[^>]*><i[^>]*></i>Guardar", form);
        Assert.Matches(@"class=""btn btn-danger""[^>]*><i[^>]*></i>Cancelar</a>", form);
        Assert.Matches(@"class=""btn btn-primary""[^>]*><i[^>]*></i>Nueva aplicaci", await _app.GetAsync("/Applications"));
        Assert.Matches(@"class=""btn w-100 py-2 btn-primary""", await _app.GetAsync("/Account/Login", anonymous: true));

        // Portal: «Entendido» acepta (azul); en «quitar de fijadas», «Cancelar» va en rojo y «Quitar» también.
        var portal = await _app.GetAsync("/");
        Assert.Matches(@"class=""btn px-4 btn-primary""[^>]*><i[^>]*></i>Entendido", portal);
        Assert.Matches(@"class=""btn btn-danger""[^>]*><i[^>]*></i>Cancelar", portal);
        Assert.Matches(@"class=""btn btn-danger""[^>]*><i[^>]*></i>Quitar", portal);
    }

    [Fact]
    public void El_boton_fijar_cambia_de_color_desde_javascript()
    {
        // Lo arma portal.js: fijar es agregar (azul) y quitar de fijadas es quitar (rojo); nada de los colores de antes.
        var script = File.ReadAllText(Path.Combine(LoadingScreenTests.ProjectFolder, "wwwroot", "js", "portal.js"));
        Assert.Contains("pinButton.classList.toggle('btn-danger', pinned)", script);
        Assert.Contains("pinButton.classList.toggle('btn-primary', !pinned)", script);
        Assert.DoesNotContain("btn-outline-danger", script);
    }

    [Theory]
    // Colores propios de site.css y portal.css sobre blanco y sobre la superficie gris clara.
    [InlineData("#1f2937", "#ffffff")]
    [InlineData("#6b7280", "#ffffff")]
    [InlineData("#1f2937", "#f8fafc")]
    [InlineData("#ffffff", "#157347")]
    [InlineData("#ffffff", "#0a58ca")]
    [InlineData("#ffffff", "#a85a00")]
    [InlineData("#ffffff", "#b02a37")]
    public void Los_colores_propios_contrastan(string text, string background)
    {
        Assert.True(MwzActionColors.Contrast(text, background) >= 4.5, $"{text} sobre {background}");
    }

    /// <summary>Una etiqueta con esas clases exactas y esa acción, en cualquier orden de atributos.</summary>
    private static string Button(string classes, string action) =>
        $@"<[^>]*(class=""{classes}""[^>]*data-mwz-accion=""{action}""|data-mwz-accion=""{action}""[^>]*class=""{classes}"")";

    /// <summary>La aplicación web con SQLite en memoria, unos datos de prueba y un administrador con sesión.</summary>
    public class App : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private HttpClient _client = null!;

        /// <summary>Valor de Mwz:Ambiente para el sitio; sin él, la etiqueta sale del ambiente Development (DEV).</summary>
        protected virtual string? Ambiente => null;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            if (Ambiente is { } ambiente)
                builder.UseSetting("Mwz:Ambiente", ambiente);
            builder.UseSetting("ConnectionStrings:QuickLinksDb", "Server=no-se-usa");
            builder.ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Warning));
            builder.ConfigureTestServices(services =>
            {
                foreach (var d in services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>)).ToList())
                    services.Remove(d);
                services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));

                services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, TestUser>(TestUser.Scheme, _ => { });
                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultScheme = TestUser.Scheme;
                    o.DefaultAuthenticateScheme = TestUser.Scheme;
                    o.DefaultChallengeScheme = TestUser.Scheme;
                });
            });
        }

        public async Task InitializeAsync()
        {
            // La base se crea antes de arrancar: al iniciar, la aplicación ya consulta los usuarios. El rol, el
            // administrador y la configuración vienen de HasData; aquí se agregan unos catálogos.
            await _connection.OpenAsync();
            await using (var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options))
            {
                await db.Database.EnsureCreatedAsync();
                var site = new Site { Name = "Planta Norte", Address = "Calle 1", CreatedByUserId = 1, CreationDate = DateTime.UtcNow, UpdatedByUserId = 1 };
                var category = new Category { Name = "Producción", UpdatedByUserId = 1 };
                var app = new Application { Name = "Inventario", Description = "Control de inventario", Link = "https://example.com/", UpdatedByUserId = 1 };
                db.AddRange(site, category, app,
                    new Category { Name = "Calidad", Available = false, UpdatedByUserId = 1 },
                    new Group { Name = "Supervisores", UpdatedByUserId = 1 });
                await db.SaveChangesAsync();
                db.AddRange(new Building { SiteId = site.Id, Name = "Edificio A", UpdatedByUserId = 1 },
                    new CategoryApplication { CategoryId = category.Id, ApplicationId = app.Id, UpdatedByUserId = 1 });
                await db.SaveChangesAsync();
            }

            _client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        public async Task<string> GetAsync(string screen, bool anonymous = false)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, screen);
            if (anonymous)
                request.Headers.Add(TestUser.AnonymousHeader, "1");
            using var response = await _client.SendAsync(request);
            var html = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"{screen}: {(int)response.StatusCode}{Environment.NewLine}{html}");
            return html;
        }

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    /// <summary>Administrador con sesión, salvo en las peticiones que piden entrar sin sesión.</summary>
    private sealed class TestUser(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string Scheme = "Prueba";
        public const string AnonymousHeader = "X-Prueba-Sin-Sesion";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey(AnonymousHeader))
                return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, AdminId.ToString()),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(AuthPolicies.RoleIdClaim, Role.AdministratorId.ToString()),
            ], Scheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme)));
        }
    }
}
