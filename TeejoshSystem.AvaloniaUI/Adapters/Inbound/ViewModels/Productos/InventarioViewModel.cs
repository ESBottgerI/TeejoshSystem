using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TeejoshInventario.Domain.Enums;
using TeejoshInventario.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Menu;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Shell;

namespace TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Productos
{
    public partial class InventarioViewModel : ObservableObject
    {
        private readonly MainViewModel _shell;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string? textoBusqueda;

        [ObservableProperty]
        private TipoProducto? tipoFiltro;

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<ProductoBusquedaDto> Productos { get; } = new();

        public List<TipoProductoOption> TiposDisponibles { get; } = new()
        {
            new TipoProductoOption { Nombre = "Todos", Valor = null },
            new TipoProductoOption { Nombre = "Hot Wheels", Valor = TipoProducto.HotWheels },
            new TipoProductoOption { Nombre = "Funko", Valor = TipoProducto.Funko },
            new TipoProductoOption { Nombre = "TCG", Valor = TipoProducto.Tcg },
            new TipoProductoOption { Nombre = "Toy", Valor = TipoProducto.Toy },
            new TipoProductoOption { Nombre = "Varios", Valor = TipoProducto.Varios }
        };

        public InventarioViewModel(
            MainViewModel shell,
            IMediator mediator,
            IServiceProvider serviceProvider)
        {
            _shell = shell;
            _mediator = mediator;
            _serviceProvider = serviceProvider;

            _ = CargarAsync();
        }

        [RelayCommand]
        private async Task CargarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var query = new BuscarProductosQuery(TextoBusqueda, TipoFiltro);
                var resultados = await _mediator.Send(query);

                Productos.Clear();
                foreach (var producto in resultados)
                    Productos.Add(producto);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargar inventario: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Volver()
        {
            var menuVm = _serviceProvider.GetRequiredService<MenuPrincipalViewModel>();
            _shell.CurrentView = menuVm;
        }
    }
}
