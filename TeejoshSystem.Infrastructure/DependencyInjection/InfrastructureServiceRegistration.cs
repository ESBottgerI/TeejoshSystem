using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Apis;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Backup;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Connectivity;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Logging;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Realtime;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Routing;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Storage;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Sync;

namespace TeejoshSystem.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Persistencia ──────────────────────────────────────────────────
            services.AddPersistence(configuration);

            var provider   = configuration["Database:Provider"] ?? "sqlite";
            var isPostgres = provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase);
            var deviceId   = GetOrCreateDeviceId();

            // ── Servicios comunes (siempre registrados) ───────────────────────
            services.AddSingleton<IAppLogger, AppLogger>();
            services.AddScoped<IAuthService, LocalAuthService>();

            services.AddHttpClient<TcgdexAdapter>();
            services.AddHttpClient<ScryfallAdapter>();
            services.AddHttpClient<YgoprodeckAdapter>();
            services.AddScoped<ITcgCatalogoApiService, TcgdexAdapter>();
            services.AddScoped<ITcgCatalogoApiService, ScryfallAdapter>();
            services.AddScoped<ITcgCatalogoApiService, YgoprodeckAdapter>();

            if (isPostgres)
                RegisterPostgresMode(services, configuration, deviceId);
            else
                RegisterSqliteMode(services, configuration, deviceId);

            return services;
        }

        // ── Modo PostgreSQL — Blazor VPS ──────────────────────────────────────
        private static void RegisterPostgresMode(
            IServiceCollection services,
            IConfiguration configuration,
            string deviceId)
        {
            var supabaseUrl     = RequireConfig(configuration, "Supabase:Url");
            var supabaseKey     = RequireConfig(configuration, "Supabase:ServiceKey");
            var supabaseAnonKey = configuration["Supabase:AnonKey"] ?? supabaseKey;
            var bucketName      = configuration["Supabase:BucketName"] ?? "product-images";

            // Storage
            services.AddSingleton<IImageStorageService>(_ =>
                new SupabaseImageStorageService(supabaseUrl, supabaseKey, bucketName));

            // Conectividad
            services.AddSingleton<SupabaseConnectivityService>(sp =>
                new SupabaseConnectivityService(supabaseUrl, supabaseAnonKey));
            services.AddSingleton<IConnectivityService>(
                sp => sp.GetRequiredService<SupabaseConnectivityService>());
            services.AddHostedService(
                sp => sp.GetRequiredService<SupabaseConnectivityService>());

            // Outbox
            services.AddScoped<ISyncOutboxRepository, SyncOutboxRepository>();

            // Sync
            services.AddSingleton(sp => new SyncService(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                supabaseUrl, supabaseKey, deviceId));
            services.AddHostedService(sp => sp.GetRequiredService<SyncService>());

            // Catalog Refresh + Realtime
            services.AddHostedService<CatalogRefreshService>();

            services.AddSingleton<SupabaseRealtimeService>(sp =>
                new SupabaseRealtimeService(supabaseUrl, supabaseAnonKey));
            services.AddSingleton<IRealtimeService>(
                sp => sp.GetRequiredService<SupabaseRealtimeService>());
            services.AddHostedService(
                sp => sp.GetRequiredService<SupabaseRealtimeService>());

            // Repositorios con routing online/offline
            services.AddScoped<IProductoRepository>(sp => new RoutingProductoRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<ISyncOutboxRepository>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<LocalDbContext>(),
                deviceId));

            services.AddScoped<IVentaRepository>(sp => new RoutingVentaRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<ISyncOutboxRepository>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<LocalDbContext>(),
                deviceId));

            services.AddScoped<ICatalogoRepository>(sp => new RoutingCatalogoRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<LocalDbContext>()));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        }

        // ── Modo SQLite — Avalonia desktop ────────────────────────────────────
        private static void RegisterSqliteMode(
            IServiceCollection services,
            IConfiguration configuration,
            string deviceId)
        {
            services.AddSingleton<IImageStorageService, LocalImageStorageService>();
            services.AddHostedService<BackupService>();

            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:ServiceKey"];
            var hasSupabase = !string.IsNullOrEmpty(supabaseUrl)
                        && !string.IsNullOrEmpty(supabaseKey);

            Console.WriteLine($"[DI] hasSupabase={hasSupabase}, supabaseUrl={supabaseUrl ?? "(null)"}");

            if (!hasSupabase)
            {
                // Sin Supabase — repositorios directos, sin routing ni sync
                services.AddScoped<IProductoRepository, ProductoRepository>();
                services.AddScoped<IVentaRepository, VentaRepository>();
                services.AddScoped<ICatalogoRepository, CatalogoRepository>();
                services.AddScoped<IUsuarioRepository, UsuarioRepository>();

                return;
            }

            // Con Supabase — SQLite local + sincronización en background
            var supabaseAnonKey = configuration["Supabase:AnonKey"] ?? supabaseKey;

            // LocalDbContext apunta al mismo SQLite para sync_outbox
            services.AddDbContext<LocalDbContext>(options =>
                PersistenceServiceRegistration.ConfigureSqlite(options));

            // Conectividad
            services.AddSingleton<SupabaseConnectivityService>(sp =>
                new SupabaseConnectivityService(
                    supabaseUrl!, supabaseAnonKey!, pingIntervalSeconds: 15));
            services.AddSingleton<IConnectivityService>(
                sp => sp.GetRequiredService<SupabaseConnectivityService>());
            services.AddHostedService(
                sp => sp.GetRequiredService<SupabaseConnectivityService>());

            // Outbox
            services.AddScoped<ISyncOutboxRepository, SyncOutboxRepository>();

            // SyncService
            services.AddSingleton(sp => new SyncService(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                supabaseUrl!, supabaseKey!, deviceId));
            services.AddHostedService(sp => sp.GetRequiredService<SyncService>());

            // Repositorios con routing — escriben en SQLite local y encolan en outbox
            // El SyncService replica el outbox a Supabase cuando hay conectividad
            services.AddScoped<IProductoRepository>(sp => new RoutingProductoRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<ISyncOutboxRepository>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<InventarioDbContext>(),
                deviceId));

            services.AddScoped<IVentaRepository>(sp => new RoutingVentaRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<ISyncOutboxRepository>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<InventarioDbContext>(),
                deviceId));

            services.AddScoped<ICatalogoRepository>(sp => new RoutingCatalogoRepository(
                sp.GetRequiredService<IConnectivityService>(),
                sp.GetRequiredService<InventarioDbContext>(),
                sp.GetRequiredService<InventarioDbContext>()));

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        }

        private static string GetOrCreateDeviceId()
        {
            var dir    = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeejoshSystem");
            var idFile = Path.Combine(dir, "device.id");
            Directory.CreateDirectory(dir);

            if (File.Exists(idFile))
                return File.ReadAllText(idFile).Trim();

            var id = Guid.NewGuid().ToString();
            File.WriteAllText(idFile, id);
            return id;
        }

        private static string RequireConfig(IConfiguration config, string key)
            => config[key] ?? throw new InvalidOperationException(
                $"Configuración requerida ausente: '{key}'.");
    }
}