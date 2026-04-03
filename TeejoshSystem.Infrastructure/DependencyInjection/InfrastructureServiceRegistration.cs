using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;
using TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshInventario.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            // DbContext
            services.AddDbContext<InventarioDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<ICatalogoRepository, CatalogoRepository>();

            return services;
        }
    }
}
