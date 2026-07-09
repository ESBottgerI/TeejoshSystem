using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TeejoshSystem.BlazorUI.Components;
using TeejoshSystem.BlazorUI.Extensions;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Fase 0: integración con Application/Infrastructure ─────────────────────
builder.Services.AddTeejoshApplicationAndInfrastructure(builder.Configuration);

// ── Fase 2 (corregida): autenticación por cookie ────────────────────────────
builder.Services.AddTeejoshAuthentication();

var app = builder.Build();

// ── Verificación de conectividad a la base de datos ─────────────────────────
// Deliberadamente NO se llama a db.Database.Migrate() aquí — ver Fase 0.3:
// Avalonia es responsable de migrar, para evitar condiciones de carrera
// contra la misma DB.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        var puedeConectar = await db.Database.CanConnectAsync();
        logger.LogInformation(
            "BlazorUI: conexión a base de datos {Estado}.",
            puedeConectar ? "verificada correctamente" : "NO disponible");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "BlazorUI: error verificando la conexión a la base de datos.");
    }
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Orden importante: Authentication/Authorization ANTES de Antiforgery y de
// mapear los componentes — es justamente lo que faltaba y causaba el
// InvalidOperationException al entrar por URL directa.
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Endpoint de logout: acción de bajo riesgo (no hace nada sensible salvo
// cerrar la sesión propia), por eso no se le exige token antiforgery — de
// lo contrario habría que registrar ese endpoint como parte de Razor
// Components para heredar la validación automática, complejidad innecesaria
// para esta acción.
app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/login");
});

app.Run();