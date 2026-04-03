using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;
using TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshInventario.Infrastructure.DependencyInjection
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<InventarioDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(InventarioDbContext).Assembly.FullName)));

            // Repositorios
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<ICatalogoRepository, CatalogoRepository>();

            return services;
        }
    }
}
