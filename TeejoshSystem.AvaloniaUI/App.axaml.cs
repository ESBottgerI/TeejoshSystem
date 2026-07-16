using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Auth;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Catalogos;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.DependencyInjection;

namespace TeejoshSystem.AvaloniaUI;

public partial class App : Avalonia.Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // ── Fase 1: Configuración ─────────────────────────────────────────────
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        var basePath = Path.GetDirectoryName(Environment.ProcessPath!)!;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // ── Fase 2: Logging ───────────────────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions(
                typeof(ConsoleLoggerConfigurationExtensions).Assembly,
                typeof(Serilog.Sinks.File.FileSink).Assembly))
            .CreateLogger();

        Log.Information("TeejoshSystem arrancando. Ambiente: {Environment}", environment);
        Log.Information("Provider: {Provider}", configuration["Database:Provider"] ?? "sqlite");

        // ── Fase 3: Construcción del host ─────────────────────────────────────
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = false;  // igual que Blazor — IAuditLogRepository pendiente
                options.ValidateScopes = false;
            })
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructure(configuration);

                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(
                        typeof(TeejoshSystem.Application.Common.Result).Assembly));

                // UI Services
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IConfirmationService, ConfirmationService>();
                services.AddSingleton<IThemePreferenceService, ThemePreferenceService>();
                services.AddSingleton<NavigationService>();
                services.AddSingleton<INavigationService>(sp =>
                    sp.GetRequiredService<NavigationService>());

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MenuPrincipalViewModel>();
                services.AddTransient<GestionarProductosViewModel>();
                services.AddTransient<CrearProductoViewModel>();
                services.AddTransient<SincronizarCatalogosViewModel>();
                services.AddTransient<AuditLogViewModel>();

                // Auth
                services.AddSingleton<SesionContext>();
                services.AddSingleton<ICurrentUserProvider, SessionCurrentUserProvider>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<GestionarUsuariosViewModel>();
                services.AddTransient<CambiarPasswordViewModel>();
            })
            .Build();

        // ── Fase 4: Migraciones y seeding ─────────────────────────────────────
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            db.Database.Migrate();
            DatabaseSeeder.SeedUsuarioAdmin(db);

            var localDb = scope.ServiceProvider.GetService<LocalDbContext>();
            localDb?.Database.EnsureCreated();
        }

        // ── Fase 5: Iniciar el host (arranca IHostedServices) ─────────────────
        // StartAsync arranca SupabaseConnectivityService, SyncService, BackupService, etc.
        // Se hace en un Task.Run para no bloquear el hilo de UI de Avalonia.
        _ = Task.Run(async () =>
        {
            await _host.StartAsync();
            Log.Information("Host iniciado. IHostedServices activos.");

            // Esperar primer ping de conectividad
            await Task.Delay(2000);

            var connectivity = _host.Services.GetService<IConnectivityService>();

            // Verificar qué implementación de IProductoRepository está registrada
            var productoRepo = _host.Services.GetService<IProductoRepository>();
            Log.Information("IProductoRepository resuelto como: {Tipo}", 
                productoRepo?.GetType().Name ?? "(null)");

            if (connectivity is not null)
                Log.Information("Conectividad inicial: {Estado}",
                    connectivity.IsOnline ? "ONLINE" : "OFFLINE");
            else
                Log.Warning("IConnectivityService no registrado — modo SQLite puro.");
        });

        // ── Fase 6: Configurar UI y navegación ────────────────────────────────
        var navService = _host.Services.GetRequiredService<NavigationService>();
        var mainVm = _host.Services.GetRequiredService<MainViewModel>();

        mainVm.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ThemeVariant))
                Dispatcher.UIThread.Post(() => RequestedThemeVariant = mainVm.ThemeVariant);
        };

        _ = Task.Run(async () =>
        {
            await mainVm.InitializeAsync();
            Dispatcher.UIThread.Post(() => RequestedThemeVariant = mainVm.ThemeVariant);
        });

        navService.Configure(
            vm => mainVm.CurrentView = vm,
            () => mainVm.CurrentView = _host.Services.GetRequiredService<MenuPrincipalViewModel>()
        );

        var sesionContext = _host.Services.GetRequiredService<SesionContext>();
        var loginVm = _host.Services.GetRequiredService<LoginViewModel>();

        loginVm.OnLoginExitoso = () =>
            mainVm.CurrentView = _host.Services.GetRequiredService<MenuPrincipalViewModel>();

        mainVm.CurrentView = sesionContext.EstaAutenticado
            ? _host.Services.GetRequiredService<MenuPrincipalViewModel>()
            : (object)loginVm;

        // ── Fase 7: Ventana principal ─────────────────────────────────────────
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainVm;
            desktop.MainWindow = mainWindow;

            desktop.Exit += async (_, _) =>
            {
                Log.Information("Cerrando TeejoshSystem...");
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
                Log.CloseAndFlush();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
