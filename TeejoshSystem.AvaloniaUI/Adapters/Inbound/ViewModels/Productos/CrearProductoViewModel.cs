using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerImagenExpansion;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

public partial class CrearProductoViewModel : ValidatableViewModel, ILoadable
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;
    private readonly IImageStorageService _imageStorage;

    // Propiedades comunes
    [ObservableProperty]
    private string? _nombre;

    [ObservableProperty]
    private decimal _precio;

    [ObservableProperty]
    private int _unidades;

    [ObservableProperty]
    private TipoProductoFiltroItem? _tipoSeleccionado;

    [ObservableProperty]
    private bool _catalogosCargados;

    [ObservableProperty]
    private string? _imagePath;

    [ObservableProperty]
    private string? _imageNombre;

    // Hot Wheels
    [ObservableProperty]
    private string? _hwModelo;

    [ObservableProperty]
    private int _hwAnio = DateTime.Now.Year;

    [ObservableProperty]
    private string? _hwSerie;

    [ObservableProperty]
    private CatalogoItemDto? _hwCategoriaSeleccionada;

    public ObservableCollection<CatalogoItemDto> CategoriasHotWheels { get; } = new();

    // Funko
    [ObservableProperty]
    private int _funkoNumeroBox;

    [ObservableProperty]
    private string? _funkoLicencia;

    [ObservableProperty]
    private CatalogoItemDto? _funkoSubtipoSeleccionado;

    [ObservableProperty]
    private CatalogoItemDto? _funkoCaracteristicaSeleccionada;

    public ObservableCollection<CatalogoItemDto> SubtiposFunko { get; } = new();
    public ObservableCollection<CatalogoItemDto> CaracteristicasFunko { get; } = new();

    // TCG
    [ObservableProperty]
    private CatalogoItemDto? _tcgFranquiciaSeleccionada;

    [ObservableProperty]
    private CatalogoItemDto? _tcgExpansionSeleccionada;

    [ObservableProperty]
    private CatalogoItemDto? _tcgPackSeleccionado;

    public ObservableCollection<CatalogoItemDto> FranquiciasTcg { get; } = new();
    public ObservableCollection<CatalogoItemDto> TcgExpansionesDisponibles { get; } = new();
    public ObservableCollection<CatalogoItemDto> TcgPacksDisponibles { get; } = new();

    // Toy
    [ObservableProperty]
    private int _toyEdadMinima;

    [ObservableProperty]
    private int _toyJugadoresMin = 1;

    [ObservableProperty]
    private int _toyJugadoresMax = 1;

    [ObservableProperty]
    private bool _toyEsJuegoMesa;

    // Varios
    [ObservableProperty]
    private string? _variosMarca;

    [ObservableProperty]
    private decimal _variosAlto;

    [ObservableProperty]
    private decimal _variosAncho;

    [ObservableProperty]
    private decimal? _variosLargo;

    [ObservableProperty]
    private string? _variosMaterial;

    [ObservableProperty]
    private bool _variosTieneIlustracion;

    // Visibilidad de paneles
    public bool MostrarHotWheels => TipoSeleccionado?.Valor == TipoProducto.HotWheels;
    public bool MostrarFunko => TipoSeleccionado?.Valor == TipoProducto.Funko;
    public bool MostrarTcg => TipoSeleccionado?.Valor == TipoProducto.Tcg;
    public bool MostrarToy => TipoSeleccionado?.Valor == TipoProducto.Toy;
    public bool MostrarVarios => TipoSeleccionado?.Valor == TipoProducto.Varios;

    public ObservableCollection<TipoProductoFiltroItem> TiposDisponibles { get; } = new()
    {
        new TipoProductoFiltroItem("Hot Wheels", TipoProducto.HotWheels),
        new TipoProductoFiltroItem("Funko", TipoProducto.Funko),
        new TipoProductoFiltroItem("TCG", TipoProducto.Tcg),
        new TipoProductoFiltroItem("Toy", TipoProducto.Toy),
        new TipoProductoFiltroItem("Varios", TipoProducto.Varios)
    };

    public CrearProductoViewModel(
        IMediator mediator,
        INotificationService notification,
        IConfirmationService confirmation,
        INavigationService navigation,
        IImageStorageService imageStorage)  // NUEVO
    {
        _mediator = mediator;
        _notification = notification;
        _confirmation = confirmation;
        _navigation = navigation;
        _imageStorage = imageStorage;  // NUEVO

        TipoSeleccionado = TiposDisponibles[0];

        ErrorsChanged += (_, _) => GuardarCommand.NotifyCanExecuteChanged();

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsBusy) ||
                e.PropertyName == nameof(CatalogosCargados))
            {
                GuardarCommand.NotifyCanExecuteChanged();
                ActualizarFranquiciaTcgCommand.NotifyCanExecuteChanged();
                UsarImagenExpansionCommand.NotifyCanExecuteChanged();
            }
        };

    }

    public Task LoadAsync(CancellationToken cancellationToken = default) => CargarCatalogosAsync(cancellationToken);

    private async Task CargarCatalogosAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            CatalogosCargados = false;
            var catalogos = await _mediator.Send(new ObtenerCatalogosQuery(), cancellationToken);

            CategoriasHotWheels.Clear();
            SubtiposFunko.Clear();
            CaracteristicasFunko.Clear();
            FranquiciasTcg.Clear();

            foreach (var cat in catalogos.CategoriasHotWheels)
                CategoriasHotWheels.Add(cat);
            foreach (var sub in catalogos.SubtiposFunko)
                SubtiposFunko.Add(sub);
            foreach (var car in catalogos.CaracteristicasFunko)
                CaracteristicasFunko.Add(car);
            foreach (var fra in catalogos.FranquiciasTcg)
                FranquiciasTcg.Add(fra);

            CatalogosCargados = true;
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error al cargar catálogos: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnTipoSeleccionadoChanged(TipoProductoFiltroItem? value)
    {
        OnPropertyChanged(nameof(MostrarHotWheels));
        OnPropertyChanged(nameof(MostrarFunko));
        OnPropertyChanged(nameof(MostrarTcg));
        OnPropertyChanged(nameof(MostrarToy));
        OnPropertyChanged(nameof(MostrarVarios));
    }

    partial void OnTcgFranquiciaSeleccionadaChanged(CatalogoItemDto? value)
    {
        TcgExpansionesDisponibles.Clear();
        TcgPacksDisponibles.Clear();
        TcgExpansionSeleccionada = null;
        TcgPackSeleccionado = null;
        ActualizarFranquiciaTcgCommand.NotifyCanExecuteChanged();
    }

    partial void OnTcgExpansionSeleccionadaChanged(CatalogoItemDto? value)
        => UsarImagenExpansionCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(PuedeActualizarFranquiciaTcg))]
    private Task ActualizarFranquiciaTcgAsync() =>
        CargarExpansionesYPacksAsync(TcgFranquiciaSeleccionada!.Id);

    [RelayCommand(CanExecute = nameof(PuedeUsarImagenExpansion))]
    private Task UsarImagenExpansionAsync() =>
        AsignarImagenDesdeExpansionAsync(TcgExpansionSeleccionada!.Id);

    private bool PuedeActualizarFranquiciaTcg() => TcgFranquiciaSeleccionada is not null && !IsBusy;
    private bool PuedeUsarImagenExpansion() => TcgExpansionSeleccionada is not null && !IsBusy;

    private async Task AsignarImagenDesdeExpansionAsync(int expansionId)
    {
        try
        {
            var imageUrl = await _mediator.Send(
                new ObtenerImagenExpansionQuery(expansionId));

            if (imageUrl is null) return;

            // Resolver ruta absoluta si es nombre de archivo local
            var fullPath = _imageStorage.GetFullPath(imageUrl);

            if (fullPath is not null)
            {
                // Solo asignar automáticamente si el usuario no ha elegido una imagen ya
                if (string.IsNullOrWhiteSpace(ImagePath) &&
                    string.IsNullOrWhiteSpace(ImageNombre))
                {
                    ImagePath = fullPath;
                    ImageNombre = System.IO.Path.GetFileName(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"AsignarImagenDesdeExpansionAsync error: {ex.Message}");
        }
    }

    private async Task CargarExpansionesYPacksAsync(int franquiciaId)
    {
        try
        {
            var result = await _mediator.Send(
                new ObtenerExpansionesYPacksQuery(franquiciaId));

            TcgExpansionesDisponibles.Clear();
            foreach (var exp in result.Expansiones)
                TcgExpansionesDisponibles.Add(exp);

            TcgPacksDisponibles.Clear();
            foreach (var pack in result.Packs)
                TcgPacksDisponibles.Add(pack);
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error al cargar expansiones: " + ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        ValidarTodo();

        if (IsBusy) return;

        if (HasErrors)
            return;

        var confirmar = await _confirmation.ConfirmAsync("¿Desea crear el producto?");
        if (!confirmar) return;

        try
        {
            IsBusy = true;
            var command = ConstruirCommand();
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                await _notification.ShowSuccessAsync("Producto creado exitosamente.");
                await _navigation.NavigateToMenuAsync();
            }
            else
            {
                await _notification.ShowErrorAsync(result.Error ?? "Error al crear producto.");
            }
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error al crear producto: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private CrearProductoCommand ConstruirCommand() => new()
    {
        Nombre = Nombre!,
        Precio = Precio,
        Unidades = Unidades,
        Tipo = TipoSeleccionado!.Valor!.Value,
        ImagePath = ImagePath,

        HotWheels = MostrarHotWheels && HwCategoriaSeleccionada != null
            ? new CrearHotWheelsDetalleDto(HwModelo!, HwAnio, HwSerie!, HwCategoriaSeleccionada.Id)
            : null,

        Funko = MostrarFunko && FunkoSubtipoSeleccionado != null
            ? new CrearFunkoDetalleDto(
                FunkoNumeroBox, FunkoLicencia!,
                FunkoSubtipoSeleccionado.Id,
                FunkoCaracteristicaSeleccionada?.Id)
            : null,

        Tcg = MostrarTcg && TcgPackSeleccionado != null && TcgExpansionSeleccionada != null
            ? new CrearTcgDetalleDto(TcgPackSeleccionado.Id, TcgExpansionSeleccionada.Id)
            : null,

        Toy = MostrarToy
            ? new CrearToyDetalleDto(
                ToyEdadMinima, ToyJugadoresMin, ToyJugadoresMax, ToyEsJuegoMesa)
            : null,

        Varios = MostrarVarios
            ? new CrearVariosDetalleDto(
                VariosMarca!, VariosAlto, VariosAncho,
                VariosLargo, VariosMaterial!, VariosTieneIlustracion)
            : null
    };

    [RelayCommand]
    private Task VolverAsync() => _navigation.NavigateToMenuAsync();

    private bool CanGuardar() => !HasErrors && !IsBusy && CatalogosCargados;

    private void ValidarNombre(string? value)
    {
        ClearErrors(nameof(Nombre));
        if (string.IsNullOrWhiteSpace(value))
            AddError(nameof(Nombre), "El nombre es obligatorio.");
        else if (value.Trim().Length > 100)
            AddError(nameof(Nombre), "El nombre no puede exceder 100 caracteres.");
    }

    partial void OnNombreChanged(string? value)
        => ValidarNombre(value);

    private void ValidarPrecio(decimal value)
    {
        ClearErrors(nameof(Precio));
        if (value < 0m)
            AddError(nameof(Precio), "El precio no puede ser negativo.");
    }

    partial void OnPrecioChanged(decimal value)
        => ValidarPrecio(value);

    private void ValidarUnidades(int value)
    {
        ClearErrors(nameof(Unidades));
        if (value < 0)
            AddError(nameof(Unidades), "Las unidades no pueden ser negativas.");
    }

    partial void OnUnidadesChanged(int value)
        => ValidarUnidades(value);

    private void ValidarHwModelo(string? value)
    {
        ClearErrors(nameof(HwModelo));
        if (string.IsNullOrWhiteSpace(value))
            AddError(nameof(HwModelo), "El modelo es obligatorio.");
    }

    partial void OnHwModeloChanged(string? value)
        => ValidarHwModelo(value);

    private void ValidarHwSerie(string? value)
    {
        ClearErrors(nameof(HwSerie));
        if (string.IsNullOrWhiteSpace(value))
            AddError(nameof(HwSerie), "La serie es obligatoria.");
    }

    partial void OnHwSerieChanged(string? value)
        => ValidarHwSerie(value);

    private void ValidarFunkoLicencia(string? value)
    {
        ClearErrors(nameof(FunkoLicencia));
        if (string.IsNullOrWhiteSpace(value))
            AddError(nameof(FunkoLicencia), "La licencia es obligatoria.");
    }

    partial void OnFunkoLicenciaChanged(string? value)
        => ValidarFunkoLicencia(value);

    protected override void ValidarTodo()
    {
        ValidarNombre(Nombre);

        if (MostrarHotWheels)
        {
            ValidarHwModelo(HwModelo);
            ValidarHwSerie(HwSerie);
        }

        if (MostrarFunko)
        {
            ValidarFunkoLicencia(FunkoLicencia);
        }
    }

    [RelayCommand]
    private async Task SeleccionarImagenAsync()
    {
        var ventana = (Avalonia.Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (ventana is null) return;

        var archivos = await ventana.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Seleccionar imagen del producto",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Imágenes")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png" }
                    }
                }
            });

        if (archivos.Count == 0) return;

        ImagePath = archivos[0].Path.LocalPath;
        ImageNombre = archivos[0].Name;
    }

    [RelayCommand]
    private void QuitarImagen()
    {
        ImagePath = null;
        ImageNombre = null;
    }
}
