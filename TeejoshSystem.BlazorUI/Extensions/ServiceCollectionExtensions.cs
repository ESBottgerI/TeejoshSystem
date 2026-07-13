using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using TeejoshSystem.Application.Common;
using TeejoshSystem.BlazorUI.Authentication;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Infrastructure.DependencyInjection;

namespace TeejoshSystem.BlazorUI.Extensions;

/// <summary>
/// Registro de Application (MediatR) e Infrastructure para BlazorUI.
///
/// Sigue exactamente el mismo patrón que TeejoshSystem.AvaloniaUI (ver App.axaml.cs:
/// services.AddInfrastructure(configuration) + services.AddMediatR(...)), para que
/// ambas interfaces de usuario compartan la misma configuración de acceso a datos
/// y no diverjan en cómo se resuelve el DbContext, los repositorios, etc.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTeejoshApplicationAndInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Result).Assembly));

        return services;
    }

    /// <summary>
    /// Autenticación de BlazorUI (Fase 2, corregida tras detectar que la
    /// sesión en memoria por circuito no sobrevive una navegación directa
    /// por URL — ver commit/nota de Fase 2 revisitada).
    ///
    /// Se usa cookie de ASP.NET Core, NO JWT: la cookie es el mecanismo
    /// estándar para que [Authorize] proteja tanto el primer GET (antes de
    /// que exista cualquier circuito de Blazor) como la navegación dentro
    /// de un circuito ya establecido. JWT sigue sin tener sentido aquí —
    /// resolvería un problema que no tenemos (no hay API stateless externa).
    /// </summary>
    public static IServiceCollection AddTeejoshAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";

                // Hoy no debería activarse nunca (Login ya solo deja pasar
                // administradores), pero se configura por defensa en
                // profundidad si en el futuro se relaja esa regla.
                options.AccessDeniedPath = "/login";

                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

        services.AddAuthorizationCore();

        // Necesario para que AuthenticationState llegue correctamente tanto
        // durante el render estático inicial como dentro del circuito
        // interactivo, sin tener que reimplementar ese puente a mano.
        services.AddCascadingAuthenticationState();

        services.AddScoped<ICurrentUserProvider, BlazorCurrentUserProvider>();

        return services;
    }
}