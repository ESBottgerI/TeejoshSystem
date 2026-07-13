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
            // Permite forzar el proveedor desde el CLI de EF:
            //   dotnet ef migrations add ... -- --provider postgresql
            // Si no se pasa el argumento, lee desde appsettings como antes.
            var providerFromArgs = args
                .SkipWhile(a => a != "--provider")
                .Skip(1)
                .FirstOrDefault();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .Build();

            var provider = providerFromArgs
                ?? configuration["Database:Provider"]
                ?? "sqlite";

            var optionsBuilder = new DbContextOptionsBuilder<InventarioDbContext>();
            PersistenceServiceRegistration.ConfigureProvider(optionsBuilder, provider, configuration);

            return new InventarioDbContext(optionsBuilder.Options);
        }
    }
}
