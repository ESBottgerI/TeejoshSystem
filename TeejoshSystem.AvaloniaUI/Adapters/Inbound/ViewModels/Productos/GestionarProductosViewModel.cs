using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos
{
    public partial class GestionarProductosViewModel : ObservableObject
    {
        private readonly MainViewModel _shell;
        private readonly IMediator _mediator;
        private readonly INotificationService _notification;
        private readonly IConfirmationService _confirmation;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string? textoBusqueda;

        [ObservableProperty]
        private TipoProducto? tipoFiltro;

        [ObservableProperty]
        private bool isBusy;

        public ObservableCollection<ProductoBusquedaDto> Productos { get; } = new();

        // CAMBIO: Lista normal en lugar de ObservableCollection observable
        public ObservableCollection<ProductoBusquedaDto> ProductosSeleccionados { get; } = new();

        public List<TipoProductoOption> TiposDisponibles { get; } = new()
        {
            new TipoProductoOption { Nombre = "Todos", Valor = null },
            new TipoProductoOption { Nombre = "Hot Wheels", Valor = TipoProducto.HotWheels },
            new TipoProductoOption { Nombre = "Funko", Valor = TipoProducto.Funko },
            new TipoProductoOption { Nombre = "TCG", Valor = TipoProducto.Tcg },
            new TipoProductoOption { Nombre = "Toy", Valor = TipoProducto.Toy },
            new TipoProductoOption { Nombre = "Varios", Valor = TipoProducto.Varios }
        };

        public GestionarProductosViewModel(
            MainViewModel shell,
            IMediator mediator,
            INotificationService notification,
            IConfirmationService confirmation,
            IServiceProvider serviceProvider)
        {
            _shell = shell;
            _mediator = mediator;
            _notification = notification;
            _confirmation = confirmation;
            _serviceProvider = serviceProvider;

            // Suscribirse a cambios en la coleccion
            ProductosSeleccionados.CollectionChanged += (s, e) =>
            {
                EditarCommand.NotifyCanExecuteChanged();
                EliminarCommand.NotifyCanExecuteChanged();
            };

            _ = BuscarAsync();
        }

        [RelayCommand]
        public async Task BuscarAsync()
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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(HaySeleccion))]
        private void Editar()
        {
            if (!HaySeleccion()) return;

            var productoSeleccionado = ProductosSeleccionados.First();

            // Crear DTO desde ProductoBusquedaDto
            var productoDto = new ProductoDto
            {
                Id = productoSeleccionado.Id,
                Nombre = productoSeleccionado.Nombre,
                Precio = productoSeleccionado.Precio,
                Unidades = productoSeleccionado.Unidades
            };

            // Crear EditarProductoViewModel SIN ProductosViewModel
            var editarVm = new EditarProductoViewModel(
                _shell,
                _mediator,
                this, // ← Pasar GestionarProductosViewModel en lugar de ProductosViewModel
                _notification,
                _confirmation,
                productoDto
            );

            _shell.CurrentView = editarVm;
        }

        [RelayCommand(CanExecute = nameof(HaySeleccion))]
        private async Task EliminarAsync()
        {
            if (!HaySeleccion()) return;

            var cantidad = ProductosSeleccionados.Count;
            var mensaje = cantidad == 1
                ? "¿Desea eliminar el producto seleccionado?"
                : $"¿Desea eliminar los {cantidad} productos seleccionados?";

            var confirmar = _confirmation.Confirm(mensaje);

            if (!await confirmar) return;

            try
            {
                IsBusy = true;

                var ids = ProductosSeleccionados.Select(p => p.Id).ToList();
                var command = new EliminarProductosCommand(ids);
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    await _notification.ShowSuccess(
                        cantidad == 1
                            ? "Producto eliminado correctamente"
                            : $"{cantidad} productos eliminados correctamente");

                    await BuscarAsync();
                    ProductosSeleccionados.Clear();
                }
                else
                {
                    await _notification.ShowError(result.Error ?? "Error al eliminar");
                }
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

        private bool HaySeleccion() => ProductosSeleccionados.Any();
    }

    // Clase auxiliar para ComboBox
    public class TipoProductoOption
    {
        public string Nombre { get; set; }
        public TipoProducto? Valor { get; set; }
    }
}
