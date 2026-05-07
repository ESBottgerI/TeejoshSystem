using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.ComponentModel;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;
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
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Infrastructure
                services.AddInfrastructure(configuration);

                // Application (MediatR)
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
                // services.AddTransient<InventarioViewModel>();
                services.AddTransient<GestionarProductosViewModel>();
                services.AddTransient<CrearProductoViewModel>();
                // services.AddTransient<EditarProductoViewModel>();
            })
            .Build();

        // Aplicar migraciones pendientes
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            db.Database.Migrate();
        }

        // Configurar navegacion
        var navService = _host.Services.GetRequiredService<NavigationService>();
        var mainVm = _host.Services.GetRequiredService<MainViewModel>();

        // Sincronizar tema con la App (ya que RequestedThemeVariant no soporta bindings directos)
        mainVm.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.ThemeVariant))
            {
                Dispatcher.UIThread.Post(() => this.RequestedThemeVariant = mainVm.ThemeVariant);
            }
        };

        // Inicializar tema de forma asíncrona para evitar deadlocks en el arranque
        _ = Task.Run(async () => 
        {
            await mainVm.InitializeAsync();
            Dispatcher.UIThread.Post(() => this.RequestedThemeVariant = mainVm.ThemeVariant);
        });

        navService.Configure(
            vm => mainVm.CurrentView = vm,
            () => mainVm.CurrentView = _host.Services.GetRequiredService<MenuPrincipalViewModel>()
        );

        // Navegar al menu inicial
        mainVm.CurrentView = _host.Services.GetRequiredService<MenuPrincipalViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainVm;

            desktop.MainWindow = mainWindow;

            desktop.Exit += (_, _) => _host.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
