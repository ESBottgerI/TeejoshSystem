using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Common;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Menu;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Shell;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos
{
    public partial class CrearProductoViewModel : ValidatableViewModel
    {
        private readonly MainViewModel _shell;
        private readonly IMediator _mediator;
        private readonly INotificationService _notification;
        private readonly IConfirmationService _confirmation;
        private readonly IServiceProvider _serviceProvider;


        // Propiedades comunes
        [ObservableProperty]
        private string? nombre;

        [ObservableProperty]
        private decimal precio;

        [ObservableProperty]
        private int unidades;

        [ObservableProperty]
        private TipoProducto tipoSeleccionado;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool catalogosCargados;


        // Propiedades especificas de Hot Wheels
        [ObservableProperty]
        private string? hwModelo;

        [ObservableProperty]
        private int hwAnio = DateTime.Now.Year;

        [ObservableProperty]
        private string? hwSerie;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CategoriasHotWheels))]
        private HotWheelsCategoria? hwCategoriaSeleccionada;

        public ObservableCollection<HotWheelsCategoria> CategoriasHotWheels { get; } = new();


        // Propiedades especificas de Funko
        [ObservableProperty]
        private int funkoNumeroBox;

        [ObservableProperty]
        private string? funkoLicencia;

        [ObservableProperty]
        private FunkoSubtipo? funkoSubtipoSeleccionado;

        [ObservableProperty]
        private FunkoCaracteristica? funkoCaracteristicaSeleccionada;

        public ObservableCollection<FunkoSubtipo> SubtiposFunko { get; } = new();
        public ObservableCollection<FunkoCaracteristica> CaracteristicasFunko { get; } = new();


        // Propiedades especificas de TCG
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TcgExpansionesDisponibles))]
        [NotifyPropertyChangedFor(nameof(TcgPacksDisponibles))]
        private TcgFranquicia? tcgFranquiciaSeleccionada;

        [ObservableProperty]
        private TcgExpansion? tcgExpansionSeleccionada;

        [ObservableProperty]
        private TcgPack? tcgPackSeleccionado;

        public ObservableCollection<TcgFranquicia> FranquiciasTcg { get; } = new();
        public ObservableCollection<TcgExpansion> TcgExpansionesDisponibles { get; } = new();
        public ObservableCollection<TcgPack> TcgPacksDisponibles { get; } = new();


        // Propiedades especificas de Toy
        [ObservableProperty]
        private int toyEdadMinima;

        [ObservableProperty]
        private int toyJugadoresMin = 1;

        [ObservableProperty]
        private int toyJugadoresMax = 1;

        [ObservableProperty]
        private bool toyEsJuegoMesa;


        // Propiedades especificas de Varios
        [ObservableProperty]
        private string? variosMarca;

        [ObservableProperty]
        private decimal variosAlto;

        [ObservableProperty]
        private decimal variosAncho;

        [ObservableProperty]
        private decimal? variosLargo;

        [ObservableProperty]
        private string? variosMaterial;

        [ObservableProperty]
        private bool variosTieneIlustracion;


        // Visibilidad de paneles segun tipo
        public bool MostrarHotWheels => TipoSeleccionado == TipoProducto.HotWheels;
        public bool MostrarFunko => TipoSeleccionado == TipoProducto.Funko;
        public bool MostrarTcg => TipoSeleccionado == TipoProducto.Tcg;
        public bool MostrarToy => TipoSeleccionado == TipoProducto.Toy;
        public bool MostrarVarios => TipoSeleccionado == TipoProducto.Varios;

        /*
        // Lista de tipos disponibles
        public ObservableCollection<TipoProductoOption> TiposDisponibles { get; } = new()
        {
            new TipoProductoOption { Nombre = "Hot Wheels", Valor = TipoProducto.HotWheels },
            new TipoProductoOption { Nombre = "Funko", Valor = TipoProducto.Funko },
            new TipoProductoOption { Nombre = "TCG", Valor = TipoProducto.Tcg },
            new TipoProductoOption { Nombre = "Toy", Valor = TipoProducto.Toy },
            new TipoProductoOption { Nombre = "Varios", Valor = TipoProducto.Varios }
        };
        
        public CrearProductoViewModel(
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

            TipoSeleccionado = TipoProducto.HotWheels;

            ErrorsChanged += (_, __) => GuardarCommand.NotifyCanExecuteChanged();

            // Cargar catalogos al inicializar
            _ = CargarCatalogosAsync();
        }

        private async Task CargarCatalogosAsync()
        {
            try
            {
                IsBusy = true;

                var query = new ObtenerCatalogosQuery();
                var catalogos = await _mediator.Send(query);

                // Hot Wheels
                foreach (var cat in catalogos.CategoriasHotWheels)
                    CategoriasHotWheels.Add(cat);

                // Funko
                foreach (var sub in catalogos.SubtiposFunko)
                    SubtiposFunko.Add(sub);

                foreach (var car in catalogos.CaracteristicasFunko)
                    CaracteristicasFunko.Add(car);

                // TCG
                foreach (var fra in catalogos.FranquiciasTcg)
                    FranquiciasTcg.Add(fra);

                CatalogosCargados = true;
            }
            catch (Exception ex)
            {
                await _notification.ShowError("Error al cargar catalogos: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnTipoSeleccionadoChanged(TipoProducto value)
        {
            OnPropertyChanged(nameof(MostrarHotWheels));
            OnPropertyChanged(nameof(MostrarFunko));
            OnPropertyChanged(nameof(MostrarTcg));
            OnPropertyChanged(nameof(MostrarToy));
            OnPropertyChanged(nameof(MostrarVarios));
        }

        partial void OnTcgFranquiciaSeleccionadaChanged(TcgFranquicia? value)
        {
            if (value is null)
            {
                TcgExpansionesDisponibles.Clear();
                TcgPacksDisponibles.Clear();
                return;
            }

            _ = CargarExpansionesYPacksAsync(value.Id);
        }

        private async Task CargarExpansionesYPacksAsync(int franquiciaId)
        {
            try
            {
                var query = new ObtenerExpansionesYPacksQuery(franquiciaId);
                var result = await _mediator.Send(query);

                TcgExpansionesDisponibles.Clear();
                foreach (var exp in result.Expansiones)
                    TcgExpansionesDisponibles.Add(exp);

                TcgPacksDisponibles.Clear();
                foreach (var pack in result.Packs)
                    TcgPacksDisponibles.Add(pack);
            }
            catch (Exception ex)
            {
                await _notification.ShowError("Error al cargar expansiones: " + ex.Message);
            }
        }
        */

        [RelayCommand(CanExecute = nameof(CanGuardar))]
        private async Task GuardarAsync()
        {
            if (IsBusy) return;

            var confirmar = _confirmation.Confirm("¿Desea crear el producto?");

            if (!await confirmar) return;

            try
            {
                IsBusy = true;

                var command = ConstruirCommand();
                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    await _notification.ShowSuccess("Producto creado exitosamente");
                    Volver();
                }
                else
                {
                    await _notification.ShowError(result.Error ?? "Error al crear producto");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private CrearProductoCommand ConstruirCommand()
        {
            return new CrearProductoCommand
            {
                Nombre = Nombre!,
                Precio = Precio,
                Unidades = Unidades,
                Tipo = TipoSeleccionado,

                HotWheels = TipoSeleccionado == TipoProducto.HotWheels && HwCategoriaSeleccionada != null
                    ? new CrearHotWheelsDetalleDto(
                        HwModelo!,
                        HwAnio,
                        HwSerie!,
                        HwCategoriaSeleccionada.Id)
                    : null,

                Funko = TipoSeleccionado == TipoProducto.Funko && FunkoSubtipoSeleccionado != null
                    ? new CrearFunkoDetalleDto(
                        FunkoNumeroBox,
                        FunkoLicencia!,
                        FunkoSubtipoSeleccionado.Id,
                        FunkoCaracteristicaSeleccionada?.Id)
                    : null,

                Tcg = TipoSeleccionado == TipoProducto.Tcg
                      && TcgPackSeleccionado != null
                      && TcgExpansionSeleccionada != null
                    ? new CrearTcgDetalleDto(
                        TcgPackSeleccionado.Id,
                        TcgExpansionSeleccionada.Id)
                    : null,

                Toy = TipoSeleccionado == TipoProducto.Toy
                    ? new CrearToyDetalleDto(
                        ToyEdadMinima,
                        ToyJugadoresMin,
                        ToyJugadoresMax,
                        ToyEsJuegoMesa)
                    : null,

                Varios = TipoSeleccionado == TipoProducto.Varios
                    ? new CrearVariosDetalleDto(
                        VariosMarca!,
                        VariosAlto,
                        VariosAncho,
                        VariosLargo,
                        VariosMaterial!,
                        VariosTieneIlustracion)
                    : null
            };
        }

        [RelayCommand]
        private void Volver()
        {
            var menuVm = _serviceProvider.GetRequiredService<MenuPrincipalViewModel>();
            _shell.CurrentView = menuVm;
        }

        private bool CanGuardar()
            => !HasErrors && !IsBusy && CatalogosCargados;

        partial void OnIsBusyChanged(bool value)
        {
            GuardarCommand.NotifyCanExecuteChanged();
        }

        partial void OnCatalogosCargadosChanged(bool value)
        {
            GuardarCommand.NotifyCanExecuteChanged();
        }

        // Validaciones
        partial void OnNombreChanged(string? value)
        {
            ClearErrors(nameof(Nombre));

            if (string.IsNullOrWhiteSpace(value))
                AddError(nameof(Nombre), "El nombre es obligatorio");
            else if (value.Length > 50)
                AddError(nameof(Nombre), "Maximo 50 caracteres");
        }

        partial void OnPrecioChanged(decimal value)
        {
            ClearErrors(nameof(Precio));

            if (value < 0)
                AddError(nameof(Precio), "El precio no puede ser negativo");
        }

        partial void OnUnidadesChanged(int value)
        {
            ClearErrors(nameof(Unidades));

            if (value < 0)
                AddError(nameof(Unidades), "Las unidades no pueden ser negativas");
        }
    }
}
