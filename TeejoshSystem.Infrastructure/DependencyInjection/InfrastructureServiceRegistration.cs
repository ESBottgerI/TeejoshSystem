using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Storage;

namespace TeejoshSystem.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<ICatalogoRepository, CatalogoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();

            // NUEVO
            services.AddSingleton<IImageStorageService, LocalImageStorageService>();

            return services;
        }
    }
}
