using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

using TeejoshSystem.Infrastructure.Adapters.Outbound.Apis;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Backup;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Connectivity;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Logging;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Observability;
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
            // ── Persistencia (DbContexts) ─────────────────────────────────────
            services.AddPersistence(configuration);

            var provider   = configuration["Database:Provider"] ?? "sqlite";
            var isPostgres = provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase);

            // ── Device ID ─────────────────────────────────────────────────────
            var deviceId = GetOrCreateDeviceId();

            // ── Logging ───────────────────────────────────────────────────────
            services.AddSingleton<IAppLogger, AppLogger>();

            // ── Auth ──────────────────────────────────────────────────────────
            services.AddScoped<IAuthService, LocalAuthService>();

            // ── Storage de imágenes ───────────────────────────────────────────
            if (isPostgres)
            {
                var supabaseUrl = RequireConfig(configuration, "Supabase:Url");
                var supabaseKey = RequireConfig(configuration, "Supabase:ServiceKey");
                var bucketName  = configuration["Supabase:BucketName"] ?? "product-images";

                services.AddSingleton<IImageStorageService>(_ =>
                    new SupabaseImageStorageService(supabaseUrl, supabaseKey, bucketName));
            }
            else
            {
                services.AddSingleton<IImageStorageService, LocalImageStorageService>();
            }

            // ── APIs externas TCG ─────────────────────────────────────────────
            services.AddHttpClient<TcgdexAdapter>();
            services.AddHttpClient<ScryfallAdapter>();
            services.AddHttpClient<YgoprodeckAdapter>();
            services.AddScoped<ITcgCatalogoApiService, TcgdexAdapter>();
            services.AddScoped<ITcgCatalogoApiService, ScryfallAdapter>();
            services.AddScoped<ITcgCatalogoApiService, YgoprodeckAdapter>();

            if (isPostgres)
            {
                var supabaseUrl     = RequireConfig(configuration, "Supabase:Url");
                var supabaseAnonKey = configuration["Supabase:AnonKey"]
                    ?? RequireConfig(configuration, "Supabase:ServiceKey");
                var supabaseKey     = RequireConfig(configuration, "Supabase:ServiceKey");

                // ── Conectividad ───────────────────────────────────────────────
                // Registrar la instancia concreta como Singleton para que sea compartida
                // entre IConnectivityService y AddHostedService (misma instancia, no dos).
                services.AddSingleton<SupabaseConnectivityService>(sp =>
                    new SupabaseConnectivityService(
                        supabaseUrl,
                        supabaseAnonKey,
                        pingIntervalSeconds: 15));

                // IConnectivityService resuelve la misma instancia singleton
                services.AddSingleton<IConnectivityService>(
                    sp => sp.GetRequiredService<SupabaseConnectivityService>());

                // BackgroundService también resuelve la misma instancia
                services.AddHostedService(
                    sp => sp.GetRequiredService<SupabaseConnectivityService>());

                // ── Outbox ─────────────────────────────────────────────────────
                services.AddScoped<ISyncOutboxRepository, SyncOutboxRepository>();

                // ── Sync Service ───────────────────────────────────────────────
                services.AddSingleton(sp => new SyncService(
                    sp.GetRequiredService<IConnectivityService>(),
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    supabaseUrl,
                    supabaseKey,
                    deviceId));
                services.AddHostedService(
                    sp => sp.GetRequiredService<SyncService>());

                // ── Catalog Refresh ────────────────────────────────────────────
                services.AddHostedService<CatalogRefreshService>();

                // ── Realtime ───────────────────────────────────────────────────
                services.AddSingleton<SupabaseRealtimeService>(sp =>
                    new SupabaseRealtimeService(supabaseUrl, supabaseAnonKey));

                services.AddSingleton<IRealtimeService>(
                    sp => sp.GetRequiredService<SupabaseRealtimeService>());

                services.AddHostedService(
                    sp => sp.GetRequiredService<SupabaseRealtimeService>());

                // ── Repositorios con routing online/offline ───────────────────
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
            else
            {
                // ── Modo SQLite puro (desarrollo / sin Supabase) ───────────────
                services.AddScoped<IProductoRepository, ProductoRepository>();
                services.AddScoped<IVentaRepository, VentaRepository>();
                services.AddScoped<ICatalogoRepository, CatalogoRepository>();
                services.AddScoped<IUsuarioRepository, UsuarioRepository>();

                services.AddHostedService<BackupService>();
            }

            services.AddSingleton<IApplicationMetrics, PrometheusApplicationMetrics>();

            return services;
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
                $"La configuración '{key}' es requerida cuando provider='postgresql'.");
    }
}
