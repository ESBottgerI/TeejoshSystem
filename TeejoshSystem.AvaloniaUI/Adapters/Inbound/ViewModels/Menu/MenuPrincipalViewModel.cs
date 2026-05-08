using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Auth;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Ventas;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu
{
    public partial class MenuPrincipalViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigation;
        private readonly SesionContext _sesionContext;
        private readonly INotificationService _notification;

        public MenuPrincipalViewModel(
            IServiceProvider serviceProvider,
            INavigationService navigation,
            SesionContext sesionContext,
            INotificationService notification)
        {
            _serviceProvider = serviceProvider;
            _navigation = navigation;
            _sesionContext = sesionContext;
            _notification = notification;
        }

        public bool EsAdministrador
            => _sesionContext.SesionActual?.Rol == RolUsuario.Administrador;

        // resto de métodos sin cambios
        [RelayCommand]
        private void VisualizarInventario()
        {
            var vm = new InventarioViewModel(
                _serviceProvider.GetRequiredService<IMediator>(),
                _navigation.NavigateToMenu);
            _navigation.NavigateTo(vm);
        }

        [RelayCommand]
        private void ModificarProducto()
        {
            var vm = _serviceProvider.GetRequiredService<GestionarProductosViewModel>();
            _navigation.NavigateTo(vm);
        }

        [RelayCommand]
        private void AnadirProducto()
        {
            var vm = _serviceProvider.GetRequiredService<CrearProductoViewModel>();
            _navigation.NavigateTo(vm);
        }

        [RelayCommand]
        private void RegistrarVenta()
        {
            var vm = new RegistrarVentaViewModel(
                _serviceProvider.GetRequiredService<IMediator>(),
                _serviceProvider.GetRequiredService<INotificationService>(),
                _serviceProvider.GetRequiredService<IConfirmationService>(),
                _navigation);
            _navigation.NavigateTo(vm);
        }

        [RelayCommand]
        private void VerHistorialVentas()
        {
            var vm = new HistorialVentasViewModel(
                _serviceProvider.GetRequiredService<IMediator>(),
                _serviceProvider.GetRequiredService<INotificationService>(),
                _navigation);
            _navigation.NavigateTo(vm);
        }

        [RelayCommand]
        private void IrAGestionarUsuarios()
        {
            if (_sesionContext.SesionActual?.Rol != RolUsuario.Administrador)
            {
                _ = _notification.ShowErrorAsync("Acceso restringido a administradores.");
                return;
            }
            _navigation.NavigateTo(_serviceProvider.GetRequiredService<GestionarUsuariosViewModel>());
        }

        [RelayCommand]
        private void IrACambiarPassword()
            => _navigation.NavigateTo(_serviceProvider.GetRequiredService<CambiarPasswordViewModel>());

        [RelayCommand]
        private void CerrarSesion()
        {
            _sesionContext.CerrarSesion();

            var loginVm = _serviceProvider.GetRequiredService<LoginViewModel>();

            loginVm.OnLoginExitoso = () =>
                _navigation.NavigateTo(_serviceProvider.GetRequiredService<MenuPrincipalViewModel>());

            _navigation.NavigateTo(loginVm);
        }
    }
}