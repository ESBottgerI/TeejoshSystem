using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Common.Formatting;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Ventas;

public partial class RegistrarVentaViewModel : ViewModelBase, ILoadable
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private string? _textoBusqueda;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarItemCommand))]
    private ProductoBusquedaDto? _productoSeleccionado;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AgregarItemCommand))]
    private int _cantidadSeleccionada = 1;

    public ObservableCollection<ProductoBusquedaDto> ProductosDisponibles { get; } = new();
    public ObservableCollection<ItemVentaVm> ItemsVenta { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarVentaCommand))]
    private decimal _totalVenta;

    public RegistrarVentaViewModel(
        IMediator mediator,
        INotificationService notification,
        IConfirmationService confirmation,
        INavigationService navigation)
    {
        _mediator = mediator;
        _notification = notification;
        _confirmation = confirmation;
        _navigation = navigation;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(IsBusy)) return;
            ConfirmarVentaCommand.NotifyCanExecuteChanged();
            AgregarItemCommand.NotifyCanExecuteChanged();
            BuscarProductosCommand.NotifyCanExecuteChanged();
        };
    }

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        BuscarProductosAsync(cancellationToken);

    [RelayCommand(CanExecute = nameof(PuedeBuscarProductos))]
    public async Task BuscarProductosAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var resultados = await _mediator.Send(
                new BuscarProductosQuery(TextoBusqueda, null), cancellationToken);

            ProductosDisponibles.Clear();
            foreach (var producto in resultados)
                ProductosDisponibles.Add(producto);
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error al buscar productos: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool PuedeBuscarProductos() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanAgregarItem))]
    private async Task AgregarItemAsync()
    {
        if (!CanAgregarItem()) return;

        var producto = ProductoSeleccionado!;
        var existente = ItemsVenta.FirstOrDefault(i => i.ProductoId == producto.Id);
        if (existente is not null)
        {
            var nuevaCantidad = existente.Cantidad + CantidadSeleccionada;
            if (nuevaCantidad > producto.Unidades)
            {
                await _notification.ShowErrorAsync(
                    $"Stock insuficiente. Disponible: {producto.Unidades}.");
                return;
            }
            existente.Cantidad = nuevaCantidad;
        }
        else
        {
            if (CantidadSeleccionada > producto.Unidades)
            {
                await _notification.ShowErrorAsync(
                    $"Stock insuficiente. Disponible: {producto.Unidades}.");
                return;
            }

            ItemsVenta.Add(new ItemVentaVm
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                PrecioUnitario = producto.Precio,
                Cantidad = CantidadSeleccionada,
                StockDisponible = producto.Unidades
            });
        }

        RecalcularTotal();
        CantidadSeleccionada = 1;
        ConfirmarVentaCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void QuitarItem(ItemVentaVm item)
    {
        ItemsVenta.Remove(item);
        RecalcularTotal();
        ConfirmarVentaCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanConfirmarVenta))]
    private async Task ConfirmarVentaAsync()
    {
        if (IsBusy) return;

        var confirmar = await _confirmation.ConfirmAsync(
            $"¿Confirmar venta por {SolesFormatter.Format(TotalVenta)}?");
        if (!confirmar || IsBusy) return;

        IsBusy = true;
        try
        {
            var command = new RegistrarVentaCommand(
                ItemsVenta.Select(i => new RegistrarVentaItemCommand(
                    i.ProductoId, i.Cantidad)).ToList());
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                await _notification.ShowSuccessAsync(
                    $"Venta #{result.Value} registrada correctamente.");
                await _navigation.NavigateToMenuAsync();
            }
            else
            {
                await _notification.ShowErrorAsync(
                    result.Error ?? "Error al registrar la venta.");
            }
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error al registrar la venta: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task VolverAsync() => _navigation.NavigateToMenuAsync();

    private void RecalcularTotal() =>
        TotalVenta = ItemsVenta.Sum(i => i.Subtotal);

    private bool CanAgregarItem() =>
        ProductoSeleccionado is not null && CantidadSeleccionada > 0 && !IsBusy;

    private bool CanConfirmarVenta() =>
        ItemsVenta.Count > 0 && !IsBusy;
}

public partial class ItemVentaVm : ObservableObject
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal PrecioUnitario { get; set; }
    public int StockDisponible { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _cantidad;

    public decimal Subtotal => PrecioUnitario * Cantidad;
}