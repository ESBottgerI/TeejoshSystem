using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu
{
    public partial class MenuPrincipalViewModel : ObservableObject
    {
        private readonly MainViewModel _shell;
        private readonly IServiceProvider _serviceProvider;

        public MenuPrincipalViewModel() : this(null!, null!) { }

        public MenuPrincipalViewModel(
            MainViewModel shell,
            IServiceProvider serviceProvider)
        {
            _shell = shell;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private void AnadirProducto()
        {
            if (_shell == null || _serviceProvider == null) return; // Previene error mientras tanto
            var vm = _serviceProvider.GetRequiredService<CrearProductoViewModel>();
            _shell.CurrentView = vm;
        }

        [RelayCommand]
        private void ModificarProducto()
        {
            if (_shell == null || _serviceProvider == null) return; // Previene error mientras tanto
            var vm = _serviceProvider.GetRequiredService<GestionarProductosViewModel>();
            _shell.CurrentView = vm;
        }

        [RelayCommand]
        private void VisualizarInventario()
        {
            if (_shell == null || _serviceProvider == null) return; // Previene error mientras tanto
            var vm = _serviceProvider.GetRequiredService<InventarioViewModel>();
            _shell.CurrentView = vm;
        }
    }
}