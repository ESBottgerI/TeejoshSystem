using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.DependencyInjection
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var provider = configuration["Database:Provider"] ?? "sqlite";

            services.AddDbContext<InventarioDbContext>(options =>
                ConfigureProvider(options, provider, configuration));

            return services;
        }

        internal static void ConfigureProvider(
            DbContextOptionsBuilder options,
            string provider,
            IConfiguration configuration)
        {
            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TeejoshSystem",
                    "inventario.db");

                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                options.UseSqlite($"Data Source={dbPath}");
            }
            else if (provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = configuration["Database:ConnectionString"]
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionString es requerido cuando el proveedor es 'postgresql'.");

                options.UseNpgsql(connectionString);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Proveedor de base de datos no soportado: '{provider}'. " +
                    "Valores válidos: 'sqlite', 'postgresql'.");
            }
        }
    }
}