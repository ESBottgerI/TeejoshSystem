using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshInventario.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshInventario.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshInventario.WPF.Adapters.Inbound.Services;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Common;
using TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Shell;
using static TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Common.ValidatableViewModel;

namespace TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Productos
{
    public partial class EditarProductoViewModel
        : ValidatableViewModel, ILoadable
    {
        private readonly MainViewModel _shell;
        private readonly IMediator _mediator;
        private readonly GestionarProductosViewModel _gestionarVm;
        private readonly INotificationService _notification;
        private readonly IConfirmationService _confirmation;
        private readonly ProductoDto _producto;

        [ObservableProperty]
        private string? nombre;

        [ObservableProperty]
        private decimal precio;

        [ObservableProperty]
        private int unidades;

        [ObservableProperty]
        private bool isBusy;

        public EditarProductoViewModel(
            MainViewModel shell,
            IMediator mediator,
            GestionarProductosViewModel gestionarVm,
            INotificationService notification,
            IConfirmationService confirmation,
            ProductoDto producto)
        {
            _shell = shell;
            _mediator = mediator;
            _gestionarVm = gestionarVm;
            _notification = notification;
            _confirmation = confirmation;
            _producto = producto;

            ErrorsChanged += (_, __) => GuardarCommand.NotifyCanExecuteChanged();
        }

        public void OnLoaded()
        {
            Nombre = _producto.Nombre;
            Precio = _producto.Precio;
            Unidades = _producto.Unidades;

            System.Diagnostics.Debug.WriteLine($"EditarProducto cargado: {Nombre}");
        }

        [RelayCommand]
        private void Volver()
        {
            _shell.CurrentView = _gestionarVm;
        }

        [RelayCommand(CanExecute = nameof(CanGuardar))]
        private async Task GuardarAsync()
        {
            if (IsBusy) return;

            var confirmar = _confirmation.Confirm(
                "¿Desea guardar los cambios del producto?");

            if (!confirmar) return;

            try
            {
                IsBusy = true;

                var result = await _mediator.Send(
                    new ActualizarProductoCommand(
                        _producto.Id,
                        Nombre!,
                        Precio,
                        Unidades));

                if (result.IsSuccess)
                {
                    _notification.ShowSuccess("Producto actualizado correctamente.");

                    // Refrescar lista y volver
                    await _gestionarVm.BuscarAsync();
                    _shell.CurrentView = _gestionarVm;
                }
                else
                {
                    _notification.ShowError(
                        result.Error ?? "Ocurrió un error al guardar el producto.");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanGuardar() => !HasErrors && !IsBusy;

        partial void OnIsBusyChanged(bool value)
        {
            GuardarCommand.NotifyCanExecuteChanged();
        }

        partial void OnNombreChanged(string? value)
        {
            ClearErrors(nameof(Nombre));

            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(Nombre), "El nombre es obligatorio.");
            else if (value.Length > 50)
                AddError(nameof(Nombre), "Máximo 50 caracteres.");
        }

        partial void OnPrecioChanged(decimal value)
        {
            ClearErrors(nameof(Precio));

            if (value < 0)
                AddError(nameof(Precio), "El precio no puede ser negativo.");
        }

        partial void OnUnidadesChanged(int value)
        {
            ClearErrors(nameof(Unidades));

            if (value < 0)
                AddError(nameof(Unidades), "Las unidades no pueden ser negativas.");
        }
    }
}