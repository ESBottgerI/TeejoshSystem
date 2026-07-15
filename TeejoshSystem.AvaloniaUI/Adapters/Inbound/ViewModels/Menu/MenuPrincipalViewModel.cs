using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Auth;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Catalogos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Ventas;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;

public partial class MenuPrincipalViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigation;
    private readonly SesionContext _sesionContext;
    private readonly INotificationService _notification;
    private readonly SincronizarCatalogosViewModel _sincronizarVm;

    public MenuPrincipalViewModel(
        IServiceProvider serviceProvider,
        INavigationService navigation,
        SesionContext sesionContext,
        INotificationService notification,
        SincronizarCatalogosViewModel sincronizarVm)
    {
        _serviceProvider = serviceProvider;
        _navigation = navigation;
        _sesionContext = sesionContext;
        _notification = notification;
        _sincronizarVm = sincronizarVm;
    }

    public bool EsAdministrador =>
        _sesionContext.SesionActual?.Rol == RolUsuario.Administrador;

    public string UsuarioActivo =>
        _sesionContext.SesionActual?.NombreUsuario ?? "—";

    public string VersionApp
    {
        get
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"v{version.ToString(3)}" : "v0.2.0";
        }
    }

    [RelayCommand]
    private Task VisualizarInventarioAsync()
    {
        var vm = new InventarioViewModel(
            _serviceProvider.GetRequiredService<IMediator>(),
            () => _navigation.NavigateToMenuAsync(),
            _serviceProvider.GetRequiredService<IImagePreviewService>());
        return _navigation.NavigateToAsync(vm);
    }

    [RelayCommand]
    private async Task ModificarProductoAsync()
    {
        if (!await AutorizarAdministradorAsync()) return;
        await _navigation.NavigateToAsync(
            _serviceProvider.GetRequiredService<GestionarProductosViewModel>());
    }

    [RelayCommand]
    private async Task AnadirProductoAsync()
    {
        if (!await AutorizarAdministradorAsync()) return;
        await _navigation.NavigateToAsync(
            _serviceProvider.GetRequiredService<CrearProductoViewModel>());
    }

    [RelayCommand]
    private Task RegistrarVentaAsync()
    {
        var vm = new RegistrarVentaViewModel(
            _serviceProvider.GetRequiredService<IMediator>(),
            _serviceProvider.GetRequiredService<INotificationService>(),
            _serviceProvider.GetRequiredService<IConfirmationService>(),
            _navigation);
        return _navigation.NavigateToAsync(vm);
    }

    [RelayCommand]
    private Task VerHistorialVentasAsync()
    {
        var vm = new HistorialVentasViewModel(
            _serviceProvider.GetRequiredService<IMediator>(),
            _serviceProvider.GetRequiredService<INotificationService>(),
            _navigation);
        return _navigation.NavigateToAsync(vm);
    }

    [RelayCommand]
    private async Task IrAGestionarUsuariosAsync()
    {
        if (!await AutorizarAdministradorAsync()) return;
        await _navigation.NavigateToAsync(
            _serviceProvider.GetRequiredService<GestionarUsuariosViewModel>());
    }

    [RelayCommand]
    private Task IrACambiarPasswordAsync() =>
        _navigation.NavigateToAsync(
            _serviceProvider.GetRequiredService<CambiarPasswordViewModel>());

    [RelayCommand]
    private async Task CerrarSesionAsync()
    {
        _sesionContext.CerrarSesion();
        var loginVm = _serviceProvider.GetRequiredService<LoginViewModel>();
        loginVm.OnLoginExitoso = () => _navigation.NavigateToAsync(
            _serviceProvider.GetRequiredService<MenuPrincipalViewModel>());
        await _navigation.NavigateToAsync(loginVm);
    }

    [RelayCommand]
    private Task SincronizarCatalogosAsync() =>
        _navigation.NavigateToAsync(_sincronizarVm);

    private async Task<bool> AutorizarAdministradorAsync()
    {
        if (EsAdministrador) return true;
        await _notification.ShowErrorAsync("Acceso restringido a administradores.");
        return false;
    }
}