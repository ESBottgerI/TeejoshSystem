using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Storage;

using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Auth;

namespace TeejoshSystem.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<ICatalogoRepository, CatalogoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IAuthService, LocalAuthService>();

            // NUEVO
            services.AddSingleton<IImageStorageService, LocalImageStorageService>();

            return services;
        }
    }
}
