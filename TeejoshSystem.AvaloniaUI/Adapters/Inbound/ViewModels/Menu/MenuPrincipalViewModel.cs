using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Shell;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Productos;

namespace TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Menu
{
    public partial class MenuPrincipalViewModel : ObservableObject
    {
        private readonly MainViewModel _shell;
        private readonly IServiceProvider _serviceProvider;

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
            var vm = _serviceProvider.GetRequiredService<CrearProductoViewModel>();
            _shell.CurrentView = vm;
        }

        [RelayCommand]
        private void ModificarProducto()
        {
            var vm = _serviceProvider.GetRequiredService<GestionarProductosViewModel>();
            _shell.CurrentView = vm;
        }

        [RelayCommand]
        private void VisualizarInventario()
        {
            var vm = _serviceProvider.GetRequiredService<InventarioViewModel>();
            _shell.CurrentView = vm;
        }
    }
}
