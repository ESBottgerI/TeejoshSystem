using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;          // NUEVO
using Serilog;                               // NUEVO
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // ── DEBUG TEMPORAL ─────────────────────────────────────
        var dbProvider = configuration["Database:Provider"] ?? "(no encontrado)";
        var dbCs = configuration["Database:ConnectionString"] ?? "(no encontrado)";
        Console.WriteLine($"[STARTUP] Provider  : {dbProvider}");
        Console.WriteLine($"[STARTUP] ConnString: {dbCs[..Math.Min(50, dbCs.Length)]}...");
        // ───────────────────────────────────────────────────────

        // NUEVO — Serilog lee su configuración completa desde appsettings.json
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()                    // NUEVO — reemplaza el logging por defecto del host
            .ConfigureServices((_, services) =>
            {
                // Infrastructure (incluye registro de IAppLogger → AppLogger)
                services.AddInfrastructure(configuration);

                // Application (MediatR)
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(
                        typeof(TeejoshSystem.Application.Common.Result).Assembly));
                services.AddSingleton(TimeProvider.System);

                // UI Services
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IConfirmationService, ConfirmationService>();
                services.AddSingleton<IImagePreviewService, ImagePreviewService>();
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

                // Auth
                services.AddSingleton<SesionContext>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<GestionarUsuariosViewModel>();
                services.AddTransient<CambiarPasswordViewModel>();
            })
            .Build();

        // Aplicar migraciones pendientes
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            db.Database.Migrate();
            DatabaseSeeder.SeedUsuarioAdmin(db);
        }

        // Configurar navegación
        var navService = _host.Services.GetRequiredService<NavigationService>();
        var mainVm = _host.Services.GetRequiredService<MainViewModel>();

        mainVm.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ThemeVariant))
            {
                Dispatcher.UIThread.Post(() => RequestedThemeVariant = mainVm.ThemeVariant);
            }
        };

        mainVm.InitializeAsync().GetAwaiter().GetResult();
        RequestedThemeVariant = mainVm.ThemeVariant;

        navService.Configure(
            vm => mainVm.CurrentView = vm,
            () => mainVm.CurrentView = _host.Services.GetRequiredService<MenuPrincipalViewModel>()
        );

        // Navegación inicial con verificación de sesión
        var sesionContext = _host.Services.GetRequiredService<SesionContext>();
        var loginVm = _host.Services.GetRequiredService<LoginViewModel>();

        loginVm.OnLoginExitoso = () => navService.NavigateToAsync(
            _host.Services.GetRequiredService<MenuPrincipalViewModel>());

        mainVm.CurrentView = sesionContext.EstaAutenticado
            ? _host.Services.GetRequiredService<MenuPrincipalViewModel>()
            : (object)loginVm;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainVm;
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                _host.Dispose();
                Log.CloseAndFlush();         // NUEVO — cierra Serilog limpiamente al salir
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
