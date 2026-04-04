using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<MenuPrincipalViewModel>();
                services.AddTransient<InventarioViewModel>();
                services.AddTransient<GestionarProductosViewModel>();
                services.AddTransient<CrearProductoViewModel>();
                services.AddTransient<EditarProductoViewModel>();
            })
            .Build();

        // Aplicar migraciones pendientes
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
            db.Database.Migrate();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = _host.Services.GetRequiredService<MainViewModel>()
            };

            desktop.Exit += (_, _) => _host.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}