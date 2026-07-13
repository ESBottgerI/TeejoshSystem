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

            // ── Contexto principal (activo según proveedor) ───────────────────
            services.AddDbContext<InventarioDbContext>(options =>
                ConfigureProvider(options, provider, configuration));

            // ── LocalDbContext (SQLite) — siempre presente como réplica local ─
            // Cuando provider=sqlite, InventarioDbContext ya ES local.
            // Cuando provider=postgresql, LocalDbContext es el contexto adicional para SQLite.
            if (provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<LocalDbContext>(options =>
                    ConfigureSqlite(options));
            }

            return services;
        }

        internal static void ConfigureProvider(
            DbContextOptionsBuilder options,
            string provider,
            IConfiguration configuration)
        {
            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
                ConfigureSqlite(options);
            else if (provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
                ConfigurePostgres(options, configuration);
            else
                throw new InvalidOperationException(
                    $"Proveedor de BD no soportado: '{provider}'. Valores: 'sqlite', 'postgresql'.");
        }

        internal static void ConfigureSqlite(DbContextOptionsBuilder options)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeejoshSystem",
                "inventario.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            options.UseSqlite($"Data Source={dbPath}");
        }

        private static void ConfigurePostgres(
            DbContextOptionsBuilder options,
            IConfiguration configuration)
        {
            var connectionString = configuration["Database:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Database:ConnectionString es requerido cuando provider='postgresql'.");

            options.UseNpgsql(connectionString);
        }
    }
}