using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using TeejoshSystem.Infrastructure.DependencyInjection;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    public class InventarioDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
    {
        public InventarioDbContext CreateDbContext(string[] args)
        {
            // Carga la misma configuración que usa la app en runtime
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .Build();

            var provider = configuration["Database:Provider"] ?? "sqlite";

            var optionsBuilder = new DbContextOptionsBuilder<InventarioDbContext>();
            PersistenceServiceRegistration.ConfigureProvider(optionsBuilder, provider, configuration);

            return new InventarioDbContext(optionsBuilder.Options);
        }
    }
}