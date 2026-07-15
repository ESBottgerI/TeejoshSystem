using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using System;
using System.Threading.Tasks;
using System.Threading;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;

public partial class EditarProductoViewModel : ValidatableViewModel, ILoadable
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;
    private readonly IImageStorageService _imageStorage;
    private readonly GestionarProductosViewModel _gestionarVm;
    private readonly int _productoId;
    private readonly TipoProducto _tipo;

    [ObservableProperty]
    private string? _nombre;

    [ObservableProperty]
    private decimal _precio;

    [ObservableProperty]
    private int _unidades;

    [ObservableProperty]
    private string? _imagePath;      // ruta temporal del archivo origen

    [ObservableProperty]
    private string? _imageNombre;    // nombre para mostrar en UI

    [ObservableProperty]
    private string? _imagePathActual; // ruta absoluta de la imagen ya guardada

    public EditarProductoViewModel(
        IMediator mediator,
        INotificationService notification,
        IConfirmationService confirmation,
        INavigationService navigation,
        IImageStorageService imageStorage,
        GestionarProductosViewModel gestionarVm,
        int productoId,
        TipoProducto tipo)
    {
        _mediator = mediator;
        _notification = notification;
        _confirmation = confirmation;
        _navigation = navigation;
        _imageStorage = imageStorage;
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

    public Task LoadAsync(CancellationToken cancellationToken = default) => CargarProductoAsync();

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

            // Cargar imagen actual si existe
            ImagePathActual = null;
            ImageNombre = dto.TieneImagen ? "Imagen guardada" : null;
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
        ImagePathActual = null; // nueva imagen reemplaza la actual
    }

    [RelayCommand]
    private void QuitarImagen()
    {
        ImagePath = null;
        ImageNombre = null;
        ImagePathActual = null;
    }

    [RelayCommand]
    private Task VolverAsync() => _navigation.NavigateToAsync(_gestionarVm);

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync()
    {
        if (IsBusy) return;

        ValidarTodo();

        if (HasErrors)
            return;

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
                    Unidades,
                    ImagePath));  // nueva imagen o null si no cambió

            if (result.IsSuccess)
            {
                await _notification.ShowSuccessAsync("Producto actualizado correctamente.");
                await _gestionarVm.BuscarAsync();
                await _navigation.NavigateToAsync(_gestionarVm);
            }
            else
            {
                await _notification.ShowErrorAsync(
                    result.Error ?? "Ocurrió un error al guardar el producto.");
            }
        }
        catch (Exception ex)
        {
            await _notification.ShowErrorAsync("Error inesperado al guardar: " + ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGuardar() => !HasErrors && !IsBusy;

    private void ValidarNombre(string? value)
    {
        ClearErrors(nameof(Nombre));

        if (string.IsNullOrWhiteSpace(value))
            AddError(nameof(Nombre), "El nombre es obligatorio.");
    }

    partial void OnNombreChanged(string? value)
        => ValidarNombre(value);

    private void ValidarPrecio(decimal value)
    {
        ClearErrors(nameof(Precio));
        if (value < 0)
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

    protected override void ValidarTodo()
    {
        ValidarNombre(Nombre);
    }
}
