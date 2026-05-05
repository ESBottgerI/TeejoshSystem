using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Productos;
using TeejoshSystem.Domain.Enums;
using System.Linq;

namespace TeejoshSystem.AvaloniaUI.Tests.Productos;

// ═══════════════════════════════════════════════════════════════════════════
// GestionarProductosViewModel
//
// El constructor dispara _ = BuscarAsync() → mock de BuscarProductosQuery
// requerido antes de crear la instancia.
// TipoFiltro es TipoProductoFiltroItem, no TipoProducto.
// TextoBusqueda (no TerminoBusqueda).
// EliminarAsync es un RelayCommand que internamente llama a IConfirmationService.
// ═══════════════════════════════════════════════════════════════════════════

public class GestionarProductosViewModelTests
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;

    public GestionarProductosViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _notification = Substitute.For<INotificationService>();
        _confirmation = Substitute.For<IConfirmationService>();
        _navigation = Substitute.For<INavigationService>();

        // El constructor llama BuscarAsync() → necesita este mock
        ConfigurarBusquedaVacia();
    }

    private void ConfigurarBusquedaVacia()
    {
        _mediator
            .Send(Arg.Any<BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductoBusquedaDto>());
    }

    private GestionarProductosViewModel CrearVm()
        => new(_mediator, _notification, _confirmation, _navigation);

    // ── Constructor / BuscarAsync inicial ─────────────────────────────────────

    [Fact]
    public async Task Constructor_DebeLanzarBusquedaInicial()
    {
        var vm = CrearVm();

        // Esperar que el fire-and-forget del constructor complete
        await Task.Delay(100);

        await _mediator.Received().Send(
            Arg.Any<BuscarProductosQuery>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuscarAsync_ResultadosDevueltos_DebePopularColeccion()
    {
        var productos = new List<ProductoBusquedaDto>
        {
            new() { Id = 1, Tipo = TipoProducto.HotWheels, Nombre = "Ford GT",     Precio = 25m, Unidades = 5, DetalleResumen = "Test" },
            new() { Id = 2, Tipo = TipoProducto.Funko,     Nombre = "Pikachu 25°", Precio = 15m, Unidades = 2, DetalleResumen = "Test" }
        };

        _mediator
            .Send(Arg.Any<BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(productos);

        var vm = CrearVm();
        await vm.BuscarCommand.ExecuteAsync(null);

        vm.Productos.Should().HaveCount(2);
        vm.Productos.Should().ContainSingle(p => p.Nombre == "Ford GT");
    }

    [Fact]
    public async Task BuscarAsync_SinResultados_DebeDejarColeccionVacia()
    {
        var vm = CrearVm();
        await vm.BuscarCommand.ExecuteAsync(null);

        vm.Productos.Should().BeEmpty();
    }

    // ── Filtros ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarAsync_ConTextoBusqueda_DebeEnviarTerminoAlQuery()
    {
        var vm = CrearVm();
        vm.TextoBusqueda = "Ford";

        await vm.BuscarCommand.ExecuteAsync(null);

        await _mediator.Received().Send(
            Arg.Is<BuscarProductosQuery>(q => q.Nombre == "Ford"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuscarAsync_ConFiltroTipo_DebeEnviarTipoAlQuery()
    {
        var vm = CrearVm();
        // TipoFiltro es TipoProductoFiltroItem, no TipoProducto directo
        vm.TipoFiltro = new TipoProductoFiltroItem("Hot Wheels", TipoProducto.HotWheels);

        await vm.BuscarCommand.ExecuteAsync(null);

        await _mediator.Received().Send(
            Arg.Is<BuscarProductosQuery>(q => q.Tipo == TipoProducto.HotWheels),
            Arg.Any<CancellationToken>());
    }

    // ── Eliminación ───────────────────────────────────────────────────────────

    [Fact]
    public async Task EliminarCommand_SinSeleccion_NoDebeEjecutarse()
    {
        var vm = CrearVm();
        vm.ProductoSeleccionado = null;

        // EliminarCommand tiene CanExecute = HaySeleccion()
        vm.EliminarCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task EliminarCommand_ConfirmadoPorUsuario_DebeEnviarCommandYRecargar()
    {
        var productoSeleccionado = new ProductoBusquedaDto
        {
            Id = 5,
            Tipo = TipoProducto.Funko,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            DetalleResumen = "Test"
        };

        _confirmation.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(true);
        _mediator
            .Send(Arg.Any<EliminarProductoCommand>(), Arg.Any<CancellationToken>())
            .Returns(TeejoshSystem.Application.Common.Result.Success());

        var vm = CrearVm();
        vm.ProductoSeleccionado = productoSeleccionado;

        await vm.EliminarCommand.ExecuteAsync(null);

        await _mediator.Received(1).Send(
            Arg.Is<EliminarProductoCommand>(c => c.ProductoIds.Contains(5)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EliminarCommand_RechazadoPorUsuario_NuncaEnviaCommand()
    {
        _confirmation.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(false);

        var vm = CrearVm();
        vm.ProductoSeleccionado = new ProductoBusquedaDto
        {
            Id = 3,
            Tipo = TipoProducto.HotWheels,
            Nombre = "Test",
            Precio = 10m,
            Unidades = 1,
            DetalleResumen = "Test"
        };

        await vm.EliminarCommand.ExecuteAsync(null);

        await _mediator.DidNotReceive().Send(
            Arg.Any<EliminarProductoCommand>(),
            Arg.Any<CancellationToken>());
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// CrearProductoViewModel
//
// Hereda ValidatableViewModel — tiene HasErrors, AddError, ClearErrors.
// Constructor dispara CargarCatalogosAsync() → mock de ObtenerCatalogosQuery.
// CanGuardar() = !HasErrors && !IsBusy && CatalogosCargados.
// Las validaciones se disparan en los partial OnXxxChanged().
// ═══════════════════════════════════════════════════════════════════════════

public class CrearProductoViewModelTests
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;

    public CrearProductoViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _notification = Substitute.For<INotificationService>();
        _confirmation = Substitute.For<IConfirmationService>();
        _navigation = Substitute.For<INavigationService>();

        // Constructor dispara CargarCatalogosAsync() → mock mínimo
        _mediator
            .Send(Arg.Any<ObtenerCatalogosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new CatalogosDto
            {
                CategoriasHotWheels = new List<CatalogoItemDto>(),
                SubtiposFunko = new List<CatalogoItemDto>(),
                CaracteristicasFunko = new List<CatalogoItemDto>(),
                FranquiciasTcg = new List<CatalogoItemDto>()
            });
    }

    private CrearProductoViewModel CrearVm()
        => new(_mediator, _notification, _confirmation, _navigation);

    // ── Visibilidad de paneles ────────────────────────────────────────────────

    [Fact]
    public void TipoSeleccionado_HotWheels_SoloPanelHotWheelsVisible()
    {
        var vm = CrearVm();
        vm.TipoSeleccionado = new TipoProductoFiltroItem("Hot Wheels", TipoProducto.HotWheels);

        vm.MostrarHotWheels.Should().BeTrue();
        vm.MostrarFunko.Should().BeFalse();
        vm.MostrarTcg.Should().BeFalse();
        vm.MostrarToy.Should().BeFalse();
        vm.MostrarVarios.Should().BeFalse();
    }

    [Fact]
    public void TipoSeleccionado_Funko_SoloPanelFunkoVisible()
    {
        var vm = CrearVm();
        vm.TipoSeleccionado = new TipoProductoFiltroItem("Funko", TipoProducto.Funko);

        vm.MostrarFunko.Should().BeTrue();
        vm.MostrarHotWheels.Should().BeFalse();
    }

    // ── Validaciones ──────────────────────────────────────────────────────────

    [Fact]
    public void Nombre_CuandoVacio_DebeAgregarError()
    {
        var vm = CrearVm();

        vm.Nombre = "";

        vm.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Nombre_CuandoValido_NoDebeAgregarError()
    {
        var vm = CrearVm();

        vm.Nombre = "Ford Mustang";

        vm.GetErrors(nameof(vm.Nombre)).Cast<object>().Should().BeEmpty();
    }

    [Fact]
    public void Nombre_MayorA50Caracteres_DebeAgregarError()
    {
        var vm = CrearVm();

        vm.Nombre = new string('A', 51);

        vm.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Precio_Negativo_DebeAgregarError()
    {
        var vm = CrearVm();

        vm.Precio = -1m;

        vm.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Precio_Cero_NoDebeAgregarError()
    {
        var vm = CrearVm();

        vm.Precio = 0m;

        vm.GetErrors(nameof(vm.Precio)).Cast<object>().Should().BeEmpty();
    }

    [Fact]
    public void Unidades_Negativas_DebeAgregarError()
    {
        var vm = CrearVm();

        vm.Unidades = -1;

        vm.HasErrors.Should().BeTrue();
    }

    // ── CanGuardar ────────────────────────────────────────────────────────────

    [Fact]
    public void CanGuardar_SinCatalogosCargados_DebeSerFalse()
    {
        var vm = CrearVm();

        // CatalogosCargados empieza en false hasta que el async completa
        vm.CatalogosCargados = false;

        vm.GuardarCommand.CanExecute(null).Should().BeFalse();
    }
}