using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshSystem.Infrastructure.DependencyInjection
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
