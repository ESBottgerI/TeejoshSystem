using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries.ObtenerVentas;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ventas;

// ═══════════════════════════════════════════════════════════════════════════
// RegistrarVentaCommandHandler
//
// Constructor real: (IVentaRepository, IProductoRepository) — en ese orden.
// El handler valida stock ANTES de llamar ReducirStock, por eso el test
// de stock insuficiente no necesita que ReducirStock lance — el handler
// ya devuelve Failure con la comparación directa stock < cantidad.
//
// RegistrarVentaItemCommand usa constructor posicional (int, int).
// ═══════════════════════════════════════════════════════════════════════════

public class RegistrarVentaCommandHandlerTests
{
    private readonly IVentaRepository    _ventaRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly RegistrarVentaCommandHandler _handler;

    public RegistrarVentaCommandHandlerTests()
    {
        _ventaRepo    = Substitute.For<IVentaRepository>();
        _productoRepo = Substitute.For<IProductoRepository>();
        // Orden correcto del constructor: (IVentaRepository, IProductoRepository)
        _handler = new RegistrarVentaCommandHandler(_ventaRepo, _productoRepo);
    }

    [Fact]
    public async Task Handle_VentaValida_DebeReducirStockYPersistirVenta()
    {
        var producto = FabricarProducto(id: 1, stock: 10, precio: 25m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(Task.FromResult(1));

        // Constructor posicional: (productoId, cantidad)
        var command = new RegistrarVentaCommand(
            new List<RegistrarVentaItemCommand>
            {
                new(1, 3)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        // El handler llama UpdateAsync para persistir el stock reducido
        await _productoRepo.Received(1).UpdateAsync(Arg.Any<Producto>());
        await _ventaRepo.Received(1).AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_StockInsuficiente_DebeRetornarFailureSinPersistirVenta()
    {
        // El handler compara stock.Value < cantidad antes de llamar ReducirStock
        var producto = FabricarProducto(id: 2, stock: 1, precio: 20m);
        _productoRepo.GetByIdAsync(2).Returns(producto);

        var command = new RegistrarVentaCommand(
            new List<RegistrarVentaItemCommand>
            {
                new(2, 5)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("insuficiente");
        producto.Stock.Value.Should().Be(1); // sin cambio
        await _ventaRepo.DidNotReceive().AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_ProductoInexistente_DebeRetornarFailure()
    {
        _productoRepo.GetByIdAsync(99).Returns((Producto?)null);

        var command = new RegistrarVentaCommand(
            new List<RegistrarVentaItemCommand> { new(99, 1) });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _ventaRepo.DidNotReceive().AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_VentaConMultiplesProductos_DebeReducirStockDeTodos()
    {
        var p1 = FabricarProducto(id: 1, stock: 10, precio: 10m);
        var p2 = FabricarProducto(id: 2, stock: 5,  precio: 20m);
        _productoRepo.GetByIdAsync(1).Returns(p1);
        _productoRepo.GetByIdAsync(2).Returns(p2);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(Task.FromResult(1));

        var command = new RegistrarVentaCommand(
            new List<RegistrarVentaItemCommand>
            {
                new(1, 2),
                new(2, 3)
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        p1.Stock.Value.Should().Be(8);
        p2.Stock.Value.Should().Be(2);
        await _productoRepo.Received(2).UpdateAsync(Arg.Any<Producto>());
    }

    [Fact]
    public void Constructor_ConListaVacia_DebeArrojarArgumentException()
    {
        // RegistrarVentaCommand valida que Items no sea vacío en su constructor
        var act = () => new RegistrarVentaCommand(new List<RegistrarVentaItemCommand>());

        act.Should().Throw<ArgumentException>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Producto FabricarProducto(int id, int stock, decimal precio)
    {
        var p = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Test"),
            new Precio(precio),
            new Unidades(stock));

        typeof(Producto).GetProperty("Id")!.SetValue(p, id);
        return p;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerVentasQueryHandler
// Retorna IReadOnlyList<VentaDto> directamente — sin Result wrapper.
// IVentaRepository expone GetByFechaAsync(DateTime?, DateTime?), no GetAllAsync.
// Venta(DateTime fecha) — constructor con parámetro requerido.
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerVentasQueryHandlerTests
{
    private readonly IVentaRepository          _ventaRepo;
    private readonly ObtenerVentasQueryHandler  _handler;

    public ObtenerVentasQueryHandlerTests()
    {
        _ventaRepo = Substitute.For<IVentaRepository>();
        _handler   = new ObtenerVentasQueryHandler(_ventaRepo);
    }

    [Fact]
    public async Task Handle_ExistenVentas_DebeRetornarDtosOrdenadosPorFechaDesc()
    {
        var hoy  = DateTime.Today;
        var ayer = DateTime.Today.AddDays(-1);

        var ventas = new List<Venta>
        {
            new(ayer), // venta más antigua
            new(hoy)   // venta más reciente
        };

        _ventaRepo
            .GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(ventas);

        // ObtenerVentasQuery puede tener propiedades Desde/Hasta opcionales
        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        // Retorna IReadOnlyList<VentaDto>, no Result<>
        result.Should().HaveCount(2);
        // El handler ordena descendente: el más reciente primero
        result.First().Fecha.Should().Be(hoy);
    }

    [Fact]
    public async Task Handle_SinVentas_DebeRetornarListaVacia()
    {
        _ventaRepo
            .GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(new List<Venta>());

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}