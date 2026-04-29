using FluentAssertions;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ventas;

/// <summary>
/// Tests del handler RegistrarVenta.
/// El caso más crítico es que una venta no pueda dejar stock negativo —
/// invariante que Unidades.Decrementar() ya garantiza en dominio,
/// pero que el handler debe capturar y convertir en Result.Failure.
/// </summary>
public class RegistrarVentaCommandHandlerTests
{
    private readonly IProductoRepository _productoRepo;
    private readonly IVentaRepository _ventaRepo;
    private readonly RegistrarVentaCommandHandler _handler;

    public RegistrarVentaCommandHandlerTests()
    {
        _productoRepo = Substitute.For<IProductoRepository>();
        _ventaRepo    = Substitute.For<IVentaRepository>();
        _handler      = new RegistrarVentaCommandHandler(_productoRepo, _ventaRepo);
    }

    [Fact]
    public async Task Handle_VentaValida_DebeReducirStockYPersistirVenta()
    {
        var producto = FabricarProducto(id: 1, stock: 10, precio: 25m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(Task.CompletedTask);

        var command = new RegistrarVentaCommand
        {
            Lineas = [new LineaVentaDto { ProductoId = 1, Cantidad = 3 }]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        producto.Stock.Value.Should().Be(7);
        await _ventaRepo.Received(1).AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_StockInsuficiente_DebeRetornarFailureSinPersistirVenta()
    {
        // Invariante de dominio: nunca stock negativo
        var producto = FabricarProducto(id: 2, stock: 1, precio: 20m);
        _productoRepo.GetByIdAsync(2).Returns(producto);

        var command = new RegistrarVentaCommand
        {
            Lineas = [new LineaVentaDto { ProductoId = 2, Cantidad = 5 }]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("stock", StringComparison.OrdinalIgnoreCase);
        producto.Stock.Value.Should().Be(1); // sin cambio
        await _ventaRepo.DidNotReceive().AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_ProductoInexistente_DebeRetornarFailure()
    {
        _productoRepo.GetByIdAsync(99).Returns((Producto?)null);

        var command = new RegistrarVentaCommand
        {
            Lineas = [new LineaVentaDto { ProductoId = 99, Cantidad = 1 }]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _ventaRepo.DidNotReceive().AddAsync(Arg.Any<Venta>());
    }

    [Fact]
    public async Task Handle_VentaConMultiplosProductos_DebeReducirStockDeTodos()
    {
        var p1 = FabricarProducto(id: 1, stock: 10, precio: 10m);
        var p2 = FabricarProducto(id: 2, stock: 5,  precio: 20m);
        _productoRepo.GetByIdAsync(1).Returns(p1);
        _productoRepo.GetByIdAsync(2).Returns(p2);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(Task.CompletedTask);

        var command = new RegistrarVentaCommand
        {
            Lineas =
            [
                new LineaVentaDto { ProductoId = 1, Cantidad = 2 },
                new LineaVentaDto { ProductoId = 2, Cantidad = 3 }
            ]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        p1.Stock.Value.Should().Be(8);
        p2.Stock.Value.Should().Be(2);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Fabrica un Producto con Id asignado via reflection para simular
    /// una entidad recuperada de BD (Id != 0).
    /// </summary>
    private static Producto FabricarProducto(int id, int stock, decimal precio)
    {
        var p = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Test"),
            new Precio(precio),
            new Unidades(stock));

        // EF Core asigna Id al recuperar de BD; en tests usamos reflection
        typeof(Producto)
            .GetProperty("Id")!
            .SetValue(p, id);

        return p;
    }
}

public class ObtenerVentasQueryHandlerTests
{
    private readonly IVentaRepository _ventaRepo;
    private readonly ObtenerVentasQueryHandler _handler;

    public ObtenerVentasQueryHandlerTests()
    {
        _ventaRepo = Substitute.For<IVentaRepository>();
        _handler   = new ObtenerVentasQueryHandler(_ventaRepo);
    }

    [Fact]
    public async Task Handle_ExistenVentas_DebeRetornarDtos()
    {
        var ventas = new List<Venta> { new Venta(), new Venta() }; // ajustar si Venta tiene constructor
        _ventaRepo.GetAllAsync().Returns(ventas);

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_SinVentas_DebeRetornarListaVaciaConSuccess()
    {
        _ventaRepo.GetAllAsync().Returns(new List<Venta>());

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
