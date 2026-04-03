using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

namespace TeejoshSystem.AvaloniaUI;

public partial class App : Avalonia.Application
{
    public static IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configurar DI
        var services = new ServiceCollection();

        // Registrar servicios de infraestructura (Domain, Application, Infrastructure)
        // Aquí debes llamar a tus propios métodos de registro si los tienes
        // Por ejemplo: services.AddApplicationServices(); services.AddInfrastructureServices();

        // Registrar servicios de UI
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IConfirmationService, ConfirmationService>();

        // Registrar ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<MenuPrincipalViewModel>();
        services.AddTransient<InventarioViewModel>();
        services.AddTransient<GestionarProductosViewModel>();
        services.AddTransient<CrearProductoViewModel>();
        services.AddTransient<EditarProductoViewModel>();

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}