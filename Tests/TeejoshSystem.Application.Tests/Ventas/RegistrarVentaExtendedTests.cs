using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ventas;

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
public class RegistrarVentaExtendedTests
{
    private readonly IVentaRepository _ventaRepo = Substitute.For<IVentaRepository>();
    private readonly IProductoRepository _productoRepo = Substitute.For<IProductoRepository>();

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