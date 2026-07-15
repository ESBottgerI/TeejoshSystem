using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

public partial class InventarioViewModel : ViewModelBase, ILoadable
{
    private readonly IMediator _mediator;
    private readonly Func<Task> _volver;
    private readonly IImagePreviewService? _imagePreview;

    public ObservableCollection<InventarioProductoItemViewModel> Productos { get; } = new();

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private TipoProductoFiltroItem? _tipoFiltro;

    [ObservableProperty]
    private bool _mostrarStockGlobal;

    [ObservableProperty]
    private string? _mensajeError;

    public ObservableCollection<TipoProductoFiltroItem> TiposDisponibles { get; } = new()
    {
        new TipoProductoFiltroItem("Todos", null),
        new TipoProductoFiltroItem("Hot Wheels", TipoProducto.HotWheels),
        new TipoProductoFiltroItem("Funko", TipoProducto.Funko),
        new TipoProductoFiltroItem("TCG", TipoProducto.Tcg),
        new TipoProductoFiltroItem("Toy", TipoProducto.Toy),
        new TipoProductoFiltroItem("Varios", TipoProducto.Varios)
    };

    public InventarioViewModel(IMediator mediator, Func<Task> volver, IImagePreviewService? imagePreview = null)
    {
        _mediator = mediator;
        _volver = volver;
        _imagePreview = imagePreview;
        _tipoFiltro = TiposDisponibles[0];
    }

    public Task LoadAsync(CancellationToken cancellationToken = default) =>
        CargarProductosAsync(cancellationToken);

    public async Task CargarProductosAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;

        IsBusy = true;
        MensajeError = null;
        try
        {
            var resultado = await _mediator.Send(new BuscarProductosQuery(
                string.IsNullOrWhiteSpace(Busqueda) ? null : Busqueda,
                TipoFiltro?.Valor), cancellationToken);

            Productos.Clear();
            foreach (var item in resultado)
                Productos.Add(new InventarioProductoItemViewModel(item, MostrarStockGlobal, _imagePreview));
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo cargar el inventario: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnMostrarStockGlobalChanged(bool value)
    {
        foreach (var item in Productos)
            item.MostrarCantidad = value;
    }

    [RelayCommand]
    private void AlternarStockGlobal() => MostrarStockGlobal = !MostrarStockGlobal;

    [RelayCommand]
    private static void AlternarStockFila(InventarioProductoItemViewModel? item)
    {
        if (item is not null)
            item.MostrarCantidad = !item.MostrarCantidad;
    }

    [RelayCommand]
    private Task CargarAsync() => CargarProductosAsync();

    [RelayCommand]
    private Task VolverAsync() => _volver();
}

public partial class InventarioProductoItemViewModel : ObservableObject
{
    private readonly ProductoBusquedaDto _producto;
    private readonly IImagePreviewService? _imagePreview;

    public InventarioProductoItemViewModel(ProductoBusquedaDto producto, bool mostrarCantidad, IImagePreviewService? imagePreview = null)
    {
        _producto = producto;
        _imagePreview = imagePreview;
        _mostrarCantidad = mostrarCantidad;
    }

    public int Id => _producto.Id;
    public TipoProducto Tipo => _producto.Tipo;
    public string Nombre => _producto.Nombre;
    public decimal Precio => _producto.Precio;
    public int Unidades => _producto.Unidades;
    public string DetalleResumen => _producto.DetalleResumen;
    public bool TieneImagen => _producto.TieneImagen;
    public byte[]? ImageThumbnail => _producto.ImageThumbnail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StockTexto))]
    private bool _mostrarCantidad;


    [RelayCommand]
    private Task VerImagenAsync() => _imagePreview is null
        ? Task.CompletedTask
        : _imagePreview.ShowAsync(ImageThumbnail, Nombre);

    public string StockTexto => MostrarCantidad
        ? Unidades.ToString()
        : Unidades > 0 ? "Disponible" : "Sin stock";
}