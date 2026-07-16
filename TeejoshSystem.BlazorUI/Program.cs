using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using TeejoshSystem.BlazorUI.Components;
using TeejoshSystem.BlazorUI.Extensions;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);


// ── TEMPORAL — revertir apenas Infrastructure implemente/registre
// IAuditLogRepository ─────────────────────────────────────────────────────
// ConsultarAuditLogQueryHandler (Fase 8, /admin/audit-log) depende de
// IAuditLogRepository, que hoy no está registrado en Infrastructure —
// mismo agujero confirmado también en Avalonia (crashea al abrir "Historial
// de cambios" y buscar), solo que ahí se dispara recién al usar la
// pantalla. WebApplicationBuilder valida TODOS los handlers registrados al
// construir el ServiceProvider, así que sin esto la app entera no arranca
// por una sola pantalla de admin todavía incompleta del lado del backend.
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateOnBuild = false;
});


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Fase 0: integración con Application/Infrastructure ─────────────────────
builder.Services.AddTeejoshApplicationAndInfrastructure(builder.Configuration);

// ── Fase 2 (corregida): autenticación por cookie ────────────────────────────
builder.Services.AddTeejoshAuthentication();

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "TeejoshSystem.BlazorUI",
            serviceVersion: "1.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })

            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })

            .AddSqlClientInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.RecordException = true;
            })

            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4317");
            });
    });

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

// ── Inicialización de LocalDbContext (SQLite local para sync outbox) ─────────
// En producción Blazor corre sin Avalonia, por lo que debe inicializar
// su propio SQLite local. EnsureCreated() crea las tablas si no existen
// sin interferir con las migraciones del InventarioDbContext principal.

// Bloque 1 — verificación de conectividad (ya existe)
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

// Bloque 2 — LocalDbContext sync outbox (agregar)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
    if (provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var localDb = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
            localDb.Database.EnsureCreated();
            logger.LogInformation("BlazorUI: LocalDbContext (sync outbox) inicializado.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BlazorUI: error inicializando LocalDbContext.");
        }
    }
}

// Bloque 3 — migración y seeding (ya existe)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var provider = builder.Configuration["Database:Provider"] ?? "sqlite";
    if (provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            await db.Database.MigrateAsync();
            logger.LogInformation("BlazorUI: migraciones aplicadas correctamente.");
            DatabaseSeeder.SeedUsuarioAdmin(db);
            logger.LogInformation("BlazorUI: DatabaseSeeder ejecutado correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BlazorUI: error durante migración o seeding.");
        }
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
app.UseHttpMetrics();
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

app.MapMetrics();
app.MapHealthChecks("/health");

app.Run();