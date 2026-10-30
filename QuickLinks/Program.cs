using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using QuickLinks.Data;
using QuickLinks.Infrastructure;
using QuickLinks.Models.Entities;
using QuickLinks.Services;
using QuickLinks.Services.Auth;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Las vistas de los catálogos viven en Views/Admin/{Controller}.
builder.Services.Configure<RazorViewEngineOptions>(options =>
    options.ViewLocationFormats.Insert(0, "/Views/Admin/{1}/{0}.cshtml"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuickLinksDb")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.SectionName));
builder.Services.AddSingleton<IMediaStorage, MediaStorage>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPortalService, PortalService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "QuickLinks.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Revalida en cada petición que el usuario siga activo y con rol de administrador,
        // para que desactivar a alguien revoque su sesión de inmediato.
        options.Events.OnValidatePrincipal = async context =>
        {
            var auth = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            if (context.Principal is null || !await auth.IsStillAuthorizedAsync(context.Principal.GetUserId()))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.Administrator, policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim(AuthPolicies.RoleIdClaim, Role.AdministratorId.ToString()))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(AuthPolicies.RoleIdClaim, Role.AdministratorId.ToString())
        .Build());

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

var culture = new CultureInfo("es-MX");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture]
});

var media = app.Services.GetRequiredService<IMediaStorage>();
var mediaOptions = app.Services.GetRequiredService<IOptions<MediaOptions>>().Value;
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(media.PhysicalRoot),
    RequestPath = mediaOptions.RequestPath,
    OnPrepareResponse = context =>
    {
        // Las imágenes subidas (incluido SVG) no pueden ejecutar scripts aunque se abran directamente.
        context.Context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
    }
});

// Un archivo de medios inexistente responde 404 en lugar de caer en la autorización y redirigir al login.
app.MapWhen(
    context => context.Request.Path.StartsWithSegments(mediaOptions.RequestPath),
    branch => branch.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await DbInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);

app.Run();
