using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Ventas.Commands;

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
    private readonly IVentaRepository _ventaRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly RegistrarVentaCommandHandler _handler;

    public RegistrarVentaCommandHandlerTests()
    {
        _ventaRepo = Substitute.For<IVentaRepository>();
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
        var p2 = FabricarProducto(id: 2, stock: 5, precio: 20m);
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

// ═══════════════════════════════════════════════════════════════════════════
// RegistrarVentaCommand — constructor
//
// Mutantes objetivo:
//   - items is null || items.Count == 0 → boundary lista vacía vs nula
//   - operador || → && (mutante clásico en OR compuesto)
// ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_ConListaVacia_DebeArrojarArgumentException()
    {
        // Boundary: lista vacía — mata mutante Count == 0 → Count != 0
        var act = () => new RegistrarVentaCommand(new List<RegistrarVentaItemCommand>());

        act.Should().Throw<ArgumentException>()
           .WithMessage("*al menos un item*");
    }

    [Fact]
    public void Constructor_ConNull_DebeArrojarArgumentException()
    {
        // Mata el mutante que evalúa solo Count (null lanzaría NullReferenceException antes)
        var act = () => new RegistrarVentaCommand(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ConUnItem_DebeCrearInstanciaCorrectamente()
    {
        // Boundary válido: exactamente 1 item
        var items = new List<RegistrarVentaItemCommand> { new(1, 2) };

        var command = new RegistrarVentaCommand(items);

        command.Items.Should().HaveCount(1);
        command.Items[0].ProductoId.Should().Be(1);
        command.Items[0].Cantidad.Should().Be(2);
    }

    [Fact]
    public void Constructor_ConVariosItems_AsignaCorrectamente()
    {
        var items = new List<RegistrarVentaItemCommand>
        {
            new(1, 3),
            new(2, 1)
        };

        var command = new RegistrarVentaCommand(items);

        command.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests extendidos de RegistrarVentaCommandHandler.
    /// Los tests base (stock insuficiente, producto inexistente, múltiples productos)
    /// ya existen en VentaHandlerTests.cs — este archivo apunta a los 16 survived / 4 no cov.
    ///
    /// Mutantes objetivo:
    ///   - producto.Stock.Value &lt; item.Cantidad → boundary stock == cantidad (válido)
    ///   - productos.First(p => p.Id == item.ProductoId) → predicado mutado
    ///   - VentaDetalle args: ProductoId, NombreProducto, Cantidad, PrecioUnitario
    ///   - venta.AgregarDetalle(detalle) → no llamado
    ///   - ventaId de AddAsync → retornado correctamente
    ///   - producto.ReducirStock(item.Cantidad) → arg mutado
    ///   - UpdateAsync → arg correcto
    ///   - catch ArgumentException/InvalidOperationException → Failure con ex.Message
    ///   - catch Exception genérica → Failure con mensaje fijo
    /// </summary>

    private RegistrarVentaCommandHandler CrearHandler()
        => new(_ventaRepo, _productoRepo);
    

    private static Producto FabricarProducto(int id, int stock, decimal precio, string nombre = "Test")
    {
        var p = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto(nombre),
            new Precio(precio),
            new Unidades(stock));
        typeof(Producto).GetProperty("Id")!.SetValue(p, id);
        return p;
    }

    // ── Boundary: stock == cantidad es válido ─────────────────────────────────

    [Fact]
    public async Task Handle_StockExactoIgualACantidad_RetornaSuccess()
    {
        // Mata mutante: stock.Value < cantidad → stock.Value <= cantidad
        // Con <=, este caso fallaría. Con <, debe pasar.
        var producto = FabricarProducto(id: 1, stock: 3, precio: 10m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(1);
        _productoRepo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 3) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        producto.Stock.Value.Should().Be(0);
    }

    // ── VentaDetalle: argumentos pasados correctamente ────────────────────────

    [Fact]
    public async Task Handle_VentaDetalle_RecibeLosArgumentosCorrectos()
    {
        // Mata mutantes: ProductoId→0, NombreProducto→null, Cantidad→0, PrecioUnitario→0
        var producto = FabricarProducto(id: 7, stock: 10, precio: 35.50m, nombre: "Charizard EX");
        _productoRepo.GetByIdAsync(7).Returns(producto);

        Venta? ventaCapturada = null;
        _ventaRepo.AddAsync(Arg.Do<Venta>(v => ventaCapturada = v)).Returns(1);
        _productoRepo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(7, 4) }),
            CancellationToken.None);

        var detalle = ventaCapturada!.Detalles.Single();
        detalle.ProductoId.Should().Be(7);
        detalle.NombreProducto.Should().Be("Charizard EX");
        detalle.Cantidad.Should().Be(4);
        detalle.PrecioUnitario.Should().Be(35.50m);
    }

    // ── productos.First: empareja por Id correcto ─────────────────────────────

    [Fact]
    public async Task Handle_DosProductos_EmparejaCadaDetalleConSuProductoCorrecto()
    {
        // Mata mutante: p.Id == item.ProductoId → predicado siempre true
        var p1 = FabricarProducto(id: 10, stock: 5, precio: 10m, nombre: "Pikachu");
        var p2 = FabricarProducto(id: 20, stock: 5, precio: 50m, nombre: "Mewtwo");
        _productoRepo.GetByIdAsync(10).Returns(p1);
        _productoRepo.GetByIdAsync(20).Returns(p2);

        Venta? ventaCapturada = null;
        _ventaRepo.AddAsync(Arg.Do<Venta>(v => ventaCapturada = v)).Returns(1);
        _productoRepo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand>
            {
                new(10, 2),
                new(20, 1)
            }), CancellationToken.None);

        var detalles = ventaCapturada!.Detalles.ToList();
        detalles.Single(d => d.ProductoId == 10).PrecioUnitario.Should().Be(10m);
        detalles.Single(d => d.ProductoId == 20).PrecioUnitario.Should().Be(50m);
    }

    // ── ventaId retornado desde AddAsync ─────────────────────────────────────

    [Fact]
    public async Task Handle_RetornaElIdDeVentaDeAddAsync()
    {
        // Mata mutante: Result.Success(ventaId) → Result.Success(0)
        var producto = FabricarProducto(id: 1, stock: 5, precio: 10m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(99); // ID específico
        _productoRepo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 1) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    // ── ReducirStock: cantidad correcta ──────────────────────────────────────

    [Fact]
    public async Task Handle_ReducirStock_UsaCantidadDelItem()
    {
        // Mata mutante: producto.ReducirStock(item.Cantidad) → ReducirStock(0)
        var producto = FabricarProducto(id: 1, stock: 10, precio: 10m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(1);
        _productoRepo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 6) }),
            CancellationToken.None);

        producto.Stock.Value.Should().Be(4); // 10 - 6 = 4
    }

    // ── UpdateAsync: producto correcto ────────────────────────────────────────

    [Fact]
    public async Task Handle_UpdateAsync_RecibeLosProductosConStockReducido()
    {
        // Mata mutante: UpdateAsync(producto) → UpdateAsync(new Producto())
        var producto = FabricarProducto(id: 1, stock: 5, precio: 10m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(1);

        Producto? capturado = null;
        _productoRepo.UpdateAsync(Arg.Do<Producto>(p => capturado = p))
                     .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 2) }),
            CancellationToken.None);

        capturado!.Stock.Value.Should().Be(3); // stock reducido antes de UpdateAsync
    }

    // ── catch blocks ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionGenerica_RetornaFailureConMensajeFijo()
    {
        // Mata mutante: catch Exception → Result.Success
        _productoRepo.When(x => x.GetByIdAsync(Arg.Any<int>()))
                     .Throw(new Exception("Error de red"));

        var result = await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 1) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("registrar la venta");
    }

    [Fact]
    public async Task Handle_ArgumentException_RetornaFailureConMensajeDeExcepcion()
    {
        // Mata mutante: catch ArgumentException → propagación
        var producto = FabricarProducto(id: 1, stock: 5, precio: 10m);
        _productoRepo.GetByIdAsync(1).Returns(producto);
        _ventaRepo.AddAsync(Arg.Any<Venta>()).Returns(1);
        _productoRepo.When(x => x.UpdateAsync(Arg.Any<Producto>()))
                     .Throw(new ArgumentException("Datos inválidos"));

        var result = await CrearHandler().Handle(
            new RegistrarVentaCommand(new List<RegistrarVentaItemCommand> { new(1, 1) }),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Datos inválidos");
    }
}