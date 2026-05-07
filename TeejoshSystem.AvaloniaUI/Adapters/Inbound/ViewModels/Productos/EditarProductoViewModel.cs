using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

public partial class EditarProductoViewModel : ValidatableViewModel, ILoadable
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;
    private readonly GestionarProductosViewModel _gestionarVm;
    private readonly int _productoId;
    private readonly TipoProducto _tipo;

    [ObservableProperty]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    private string? _nombre;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
    private decimal _precio;

    [ObservableProperty]
    [Range(0, int.MaxValue, ErrorMessage = "Las unidades no pueden ser negativas.")]
    private int _unidades;

    public EditarProductoViewModel(
        IMediator mediator,
        INotificationService notification,
        IConfirmationService confirmation,
        INavigationService navigation,
        GestionarProductosViewModel gestionarVm,
        int productoId,
        TipoProducto tipo)
    {
        _mediator = mediator;
        _notification = notification;
        _confirmation = confirmation;
        _navigation = navigation;
        _gestionarVm = gestionarVm;
        _productoId = productoId;
        _tipo = tipo;

        ErrorsChanged += (_, _) => GuardarCommand.NotifyCanExecuteChanged();

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsBusy))
                GuardarCommand.NotifyCanExecuteChanged();
        };
    }

    private bool _isLoaded;

    public void OnLoaded()
    {
        _ = CargarProductoAsync();
    }

    private async Task CargarProductoAsync()
    {
        try
        {
            IsBusy = true;

            var result = await _mediator.Send(
                new ObtenerProductosPorIdQuery(_productoId));

            if (!result.IsSuccess)
            {
                await _notification.ShowErrorAsync(
                    result.Error ?? "No se pudo cargar el producto.");
                return;
            }

            var dto = result.Value;
            Nombre = dto.Nombre;
            Precio = dto.Precio;
            Unidades = dto.Unidades;
            
            await Task.Delay(100);
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await _notification.ShowErrorAsync("Error inesperado al cargar el producto.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Volver() => _navigation.NavigateTo(_gestionarVm);

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        ValidateAllProperties();
        if (HasErrors || IsBusy) return;

        var confirmar = await _confirmation.ConfirmAsync(
            "¿Desea guardar los cambios del producto?");
        if (!confirmar) return;

        try
        {
            IsBusy = true;

            var result = await _mediator.Send(
                new ActualizarProductoCommand(
                    _productoId,
                    Nombre!,
                    Precio,
                    Unidades));

            if (result.IsSuccess)
            {
                await _notification.ShowSuccessAsync("Producto actualizado correctamente.");
                await _gestionarVm.BuscarAsync();
                _navigation.NavigateTo(_gestionarVm);
            }
            else
            {
                await _notification.ShowErrorAsync(
                    result.Error ?? "Ocurrió un error al guardar el producto.");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGuardar() => !HasErrors && !IsBusy;

    partial void OnNombreChanged(string? value) { if (_isLoaded) ValidateProperty(value, nameof(Nombre)); }
    partial void OnPrecioChanged(decimal value) { if (_isLoaded) ValidateProperty(value, nameof(Precio)); }
    partial void OnUnidadesChanged(int value) { if (_isLoaded) ValidateProperty(value, nameof(Unidades)); }
}