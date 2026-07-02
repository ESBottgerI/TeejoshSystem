using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Apis;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Logging;           // NUEVO
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Storage;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Backup;

namespace TeejoshSystem.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            // Repositories & services
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<ICatalogoRepository, CatalogoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IAuthService, LocalAuthService>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            services.AddSingleton<IImageStorageService, LocalImageStorageService>();

            // NUEVO — App logger (singleton: stateless, hilo-seguro, vive toda la app)
            services.AddSingleton<IAppLogger, AppLogger>();

            // HttpClient para los adapters de APIs externas
            services.AddHttpClient<TcgdexAdapter>();
            services.AddHttpClient<ScryfallAdapter>();
            services.AddHttpClient<YgoprodeckAdapter>();

            // Registrar los tres adapters como colección de ITcgCatalogoApiService
            services.AddScoped<ITcgCatalogoApiService, TcgdexAdapter>();
            services.AddScoped<ITcgCatalogoApiService, ScryfallAdapter>();
            services.AddScoped<ITcgCatalogoApiService, YgoprodeckAdapter>();

            // Backup automático sólo cuando el proveedor es SQLite
            var provider = configuration["Database:Provider"] ?? "sqlite";
            if (provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
                services.AddHostedService<BackupService>();

            return services;
        }
    }
}