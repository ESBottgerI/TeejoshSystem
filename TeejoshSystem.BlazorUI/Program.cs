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
// Antes de esto, BlazorUI era la plantilla por defecto sin ninguna conexión
// real a la base de datos ni a los Commands/Queries de Application.
builder.Services.AddTeejoshApplicationAndInfrastructure(builder.Configuration);

var app = builder.Build();

// ── Verificación de conectividad a la base de datos ─────────────────────────
// Deliberadamente NO se llama a db.Database.Migrate() aquí.
// TeejoshSystem.AvaloniaUI ya es responsable de aplicar migraciones al arrancar
// (ver App.axaml.cs: db.Database.Migrate() + DatabaseSeeder.SeedUsuarioAdmin).
// Si ambas apps intentaran migrar al mismo tiempo contra la misma base de datos
// (especialmente en producción, sobre Postgres/Supabase), se genera una
// condición de carrera real. Blazor solo verifica que puede conectarse;
// si en el futuro se decide que Blazor sea el responsable de migrar, debe ser
// una decisión explícita del equipo, no un efecto secundario de copiar código.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        var puedeConectar = await db.Database.CanConnectAsync();

        if (puedeConectar)
        {
            logger.LogInformation("BlazorUI: conexión a base de datos verificada correctamente.");
        }
        else
        {
            logger.LogWarning("BlazorUI: no fue posible conectar a la base de datos al arrancar.");
        }
    }
    catch (Exception ex)
    {
        // No se detiene el arranque de la app por esto: se registra y se deja
        // que las páginas individuales fallen visiblemente si intentan usar
        // Mediator.Send sin conexión, en vez de tumbar el proceso completo.
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();