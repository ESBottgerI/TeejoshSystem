using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.WebUI.Infrastructure.Auth;
using TeejoshSystem.WebUI.Infrastructure.Services;
using TeejoshSystem.WebUI.Infrastructure.State;

namespace TeejoshSystem.WebUI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTeejoshWebUi(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddAuthentication(BlazorCircuitAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, BlazorCircuitAuthenticationHandler>(
                BlazorCircuitAuthenticationHandler.SchemeName,
                options => { });
        services.AddAuthenticationCore();
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddScoped<BlazorSesionContext>();
        services.AddScoped<CustomAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthenticationStateProvider>());
        services.AddScoped<BlazorUserContext>();
        services.AddScoped<CircuitStateStore>();
        services.AddScoped<BlazorNotificationService>();
        services.AddScoped<BlazorConfirmationService>();

        return services;
    }

    public static async Task ApplyTeejoshDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
        if (!configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            if (!await db.Database.CanConnectAsync())
                throw new InvalidOperationException("No se pudo conectar con la base de datos configurada.");
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
                throw new InvalidOperationException("La base de datos tiene migraciones pendientes. Ejecute el migrador antes de iniciar WebUI.");
            return;
        }

        await db.Database.MigrateAsync();
        DatabaseSeeder.SeedUsuarioAdmin(db);
    }
}
