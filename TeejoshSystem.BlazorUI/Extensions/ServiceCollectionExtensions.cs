using TeejoshSystem.Application.Common;
using TeejoshSystem.Infrastructure.DependencyInjection;

namespace TeejoshSystem.BlazorUI.Extensions;

/// <summary>
/// Registro de Application (MediatR) e Infrastructure para BlazorUI.
///
/// Sigue exactamente el mismo patrón que TeejoshSystem.AvaloniaUI (ver App.axaml.cs:
/// services.AddInfrastructure(configuration) + services.AddMediatR(...)), para que
/// ambas interfaces de usuario compartan la misma configuración de acceso a datos
/// y no diverjan en cómo se resuelve el DbContext, los repositorios, etc.
///
/// Deliberadamente NO se registran aquí servicios de sesión/autenticación (SesionContext,
/// ICurrentUserProvider): esos se tratan en la Fase 2 del plan, porque en Blazor Server
/// deben ser Scoped (uno por circuito de usuario), no Singleton como en Avalonia.
/// Mezclarlos aquí sería fácil de copiar mal.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTeejoshApplicationAndInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure: DbContext, repositorios, IAppLogger, etc.
        services.AddInfrastructure(configuration);

        // Application: MediatR, usando el mismo assembly marker que Avalonia
        // (Result vive en TeejoshSystem.Application.Common, en el mismo assembly
        // que todos los Commands/Queries/Handlers).
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Result).Assembly));

        return services;
    }
}