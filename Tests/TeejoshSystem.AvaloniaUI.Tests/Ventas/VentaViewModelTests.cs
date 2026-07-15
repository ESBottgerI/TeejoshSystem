using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries.ObtenerVentas;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Ventas;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.AvaloniaUI.Tests.Ventas;

// ═══════════════════════════════════════════════════════════════════════════
// HistorialVentasViewModel
//
// Constructor: (IMediator, INotificationService, INavigationService) — sin IConfirmationService.
// Constructor dispara _ = BuscarAsync() → mock de ObtenerVentasQuery requerido.
// ObtenerVentasQuery(DateTime? desde, DateTime? hasta) — no parameterless.
// BuscarAsync es RelayCommand público.
// ═══════════════════════════════════════════════════════════════════════════

public class HistorialVentasViewModelTests
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly INavigationService _navigation;

    public HistorialVentasViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _notification = Substitute.For<INotificationService>();
        _navigation = Substitute.For<INavigationService>();

        // Constructor dispara BuscarAsync → necesita este mock
        _mediator
            .Send(Arg.Any<ObtenerVentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<VentaDto>());
    }

    private HistorialVentasViewModel CrearVm()
        => new(_mediator, _notification, _navigation);

    // ── BuscarAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task BuscarAsync_ConVentas_DebePopularColeccion()
    {
        var ventas = new List<VentaDto>
        {
            new() { Id = 1, Fecha = DateTime.Today,              Total = 50m,  Detalles = new() },
            new() { Id = 2, Fecha = DateTime.Today.AddDays(-1),  Total = 30m,  Detalles = new() }
        };

        _mediator
            .Send(Arg.Any<ObtenerVentasQuery>(), Arg.Any<CancellationToken>())
            .Returns(ventas);

        var vm = CrearVm();
        await vm.BuscarCommand.ExecuteAsync(null);

        vm.Ventas.Should().HaveCount(2);
    }

    [Fact]
    public async Task BuscarAsync_SinVentas_DebeDejarColeccionVacia()
    {
        var vm = CrearVm();
        await vm.BuscarCommand.ExecuteAsync(null);

        vm.Ventas.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarAsync_ConFechaDesde_DebeEnviarFiltroAlQuery()
    {
        var desde = new DateTime(2025, 1, 1);
        var vm = CrearVm();
        vm.FechaDesde = desde;

        await vm.BuscarCommand.ExecuteAsync(null);

        await _mediator.Received().Send(
            Arg.Is<ObtenerVentasQuery>(q => q.Desde == desde),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuscarAsync_ExcepcionEnMediator_DebeNotificarError()
    {
        _mediator
            .When(x => x.Send(Arg.Any<ObtenerVentasQuery>(), Arg.Any<CancellationToken>()))
            .Throw(new Exception("Error de red"));

        var vm = CrearVm();
        await vm.BuscarCommand.ExecuteAsync(null);

        await _notification.Received().ShowErrorAsync(Arg.Any<string>());
    }

    [Fact]
    public void LimpiarFiltrosCommand_DebeResetearFechas()
    {
        var vm = CrearVm();
        vm.FechaDesde = DateTime.Today;
        vm.FechaHasta = DateTime.Today;

        vm.LimpiarFiltrosCommand.Execute(null);

        vm.FechaDesde.Should().BeNull();
        vm.FechaHasta.Should().BeNull();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RegistrarVentaViewModel
//
// Constructor dispara _ = BuscarProductosAsync() → mock de BuscarProductosQuery.
// No existe AgregarLinea(int, int, decimal) — es AgregarItemCommand (RelayCommand).
// Items están en ItemsVenta (ObservableCollection<ItemVentaVm>), no Lineas.
// TotalVenta se recalcula en RecalcularTotal() tras cada AgregarItem/QuitarItem.
// ConfirmarVentaAsync es RelayCommand interno — requiere IConfirmationService.
// ═══════════════════════════════════════════════════════════════════════════

public class RegistrarVentaViewModelTests
{
    private readonly IMediator _mediator;
    private readonly INotificationService _notification;
    private readonly IConfirmationService _confirmation;
    private readonly INavigationService _navigation;

    public RegistrarVentaViewModelTests()
    {
        _mediator = Substitute.For<IMediator>();
        _notification = Substitute.For<INotificationService>();
        _confirmation = Substitute.For<IConfirmationService>();
        _navigation = Substitute.For<INavigationService>();

        // Constructor dispara BuscarProductosAsync()
        _mediator
            .Send(Arg.Any<BuscarProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductoBusquedaDto>());
    }

    private RegistrarVentaViewModel CrearVm()
        => new(_mediator, _notification, _confirmation, _navigation);

    // ── AgregarItem ───────────────────────────────────────────────────────────

    [Fact]
    public void AgregarItemCommand_ProductoConStock_DebeAgregarACarrito()
    {
        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 10, precio: 25m);
        vm.CantidadSeleccionada = 2;

        vm.AgregarItemCommand.Execute(null);

        vm.ItemsVenta.Should().HaveCount(1);
        vm.ItemsVenta.Single().Cantidad.Should().Be(2);
        vm.ItemsVenta.Single().PrecioUnitario.Should().Be(25m);
    }

    [Fact]
    public void AgregarItemCommand_MismoProductoDosVeces_DebeSumarCantidad()
    {
        var vm = CrearVm();
        var producto = ProductoDisponible(id: 1, stock: 10, precio: 20m);

        vm.ProductoSeleccionado = producto;
        vm.CantidadSeleccionada = 2;
        vm.AgregarItemCommand.Execute(null);

        vm.ProductoSeleccionado = producto;
        vm.CantidadSeleccionada = 3;
        vm.AgregarItemCommand.Execute(null);

        // No duplica el item — suma la cantidad
        vm.ItemsVenta.Should().HaveCount(1);
        vm.ItemsVenta.Single().Cantidad.Should().Be(5);
    }

    [Fact]
    public async Task AgregarItemCommand_StockInsuficiente_DebeNotificarError()
    {
        _notification.ShowErrorAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 1, precio: 10m);
        vm.CantidadSeleccionada = 5; // supera stock

        vm.AgregarItemCommand.Execute(null);

        await _notification.Received(1).ShowErrorAsync(Arg.Any<string>());
        vm.ItemsVenta.Should().BeEmpty();
    }

    // ── TotalVenta ────────────────────────────────────────────────────────────

    [Fact]
    public void TotalVenta_DosItems_DebeSerSumaDeSubtotales()
    {
        var vm = CrearVm();

        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 10, precio: 10m);
        vm.CantidadSeleccionada = 2; // 20
        vm.AgregarItemCommand.Execute(null);

        vm.ProductoSeleccionado = ProductoDisponible(id: 2, stock: 10, precio: 15m);
        vm.CantidadSeleccionada = 3; // 45
        vm.AgregarItemCommand.Execute(null);

        vm.TotalVenta.Should().Be(65m);
    }

    [Fact]
    public void TotalVenta_SinItems_DebeSerCero()
    {
        var vm = CrearVm();

        vm.TotalVenta.Should().Be(0m);
    }

    // ── QuitarItem ────────────────────────────────────────────────────────────

    [Fact]
    public void QuitarItemCommand_DebeEliminarItemYRecalcularTotal()
    {
        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 10, precio: 20m);
        vm.CantidadSeleccionada = 1;
        vm.AgregarItemCommand.Execute(null);

        var item = vm.ItemsVenta.Single();
        vm.QuitarItemCommand.Execute(item);

        vm.ItemsVenta.Should().BeEmpty();
        vm.TotalVenta.Should().Be(0m);
    }

    // ── CanConfirmarVenta ─────────────────────────────────────────────────────

    [Fact]
    public void ConfirmarVentaCommand_SinItems_NoDebePoderEjecutarse()
    {
        var vm = CrearVm();

        vm.ConfirmarVentaCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmarVentaCommand_ConItems_DebePoderEjecutarse()
    {
        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 5, precio: 10m);
        vm.CantidadSeleccionada = 1;
        vm.AgregarItemCommand.Execute(null);

        vm.ConfirmarVentaCommand.CanExecute(null).Should().BeTrue();
    }

    // ── ConfirmarVenta ────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmarVentaCommand_Exitoso_DebeNavegar()
    {
        _confirmation.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(true);
        _mediator
            .Send(Arg.Any<RegistrarVentaCommand>(), Arg.Any<CancellationToken>())
            .Returns(TeejoshSystem.Application.Common.Result.Success(1));

        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 5, precio: 10m);
        vm.CantidadSeleccionada = 1;
        vm.AgregarItemCommand.Execute(null);

        await vm.ConfirmarVentaCommand.ExecuteAsync(null);

        await _navigation.Received(1).NavigateToMenuAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmarVentaCommand_RechazadoPorUsuario_NuncaEnviaCommand()
    {
        _confirmation.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>())
                     .Returns(false);

        var vm = CrearVm();
        vm.ProductoSeleccionado = ProductoDisponible(id: 1, stock: 5, precio: 10m);
        vm.CantidadSeleccionada = 1;
        vm.AgregarItemCommand.Execute(null);

        await vm.ConfirmarVentaCommand.ExecuteAsync(null);

        await _mediator.DidNotReceive().Send(
            Arg.Any<RegistrarVentaCommand>(),
            Arg.Any<CancellationToken>());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProductoBusquedaDto ProductoDisponible(int id, int stock, decimal precio)
        => new()
        {
            Id = id,
            Tipo = TipoProducto.HotWheels,
            Nombre = $"Producto {id}",
            Precio = precio,
            Unidades = stock,
            DetalleResumen = "Test"
        };
}