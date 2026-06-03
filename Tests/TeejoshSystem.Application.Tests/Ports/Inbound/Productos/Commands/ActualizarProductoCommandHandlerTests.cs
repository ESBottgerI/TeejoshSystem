using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Commands;

public class ActualizarProductoCommandHandlerTests
{
    private readonly IProductoRepository _repo = Substitute.For<IProductoRepository>();
    private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private ActualizarProductoCommandHandler CrearHandler()
        => new(_repo, _imageStorage, _logger);

    private static Producto ProductoExistente()
        => new(TipoProducto.HotWheels,
               new NombreProducto("Camaro"),
               new Precio(25m),
               new Unidades(5));

    // ── Happy path sin imagen ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoExisteSinImagen_RetornaSuccessYLlamaUpdateAsync()
    {
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro 1969", 30m, 10, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(Arg.Any<Producto>());
    }

    // ── Happy path con imagen ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoExisteConImagen_GuardaImagenYAsignaPath()
    {
        // Mata mutante: AsignarImagePath(imageName) → AsignarImagePath(null)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _imageStorage.SaveImageAsync("origen/camaro.jpg").Returns("guardada/camaro_001.jpg");
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 30m, 5, "origen/camaro.jpg"),
            CancellationToken.None);

        // Verifica que SaveImageAsync recibió la ruta correcta
        await _imageStorage.Received(1).SaveImageAsync("origen/camaro.jpg");

        // Verifica que el producto pasado a UpdateAsync tiene el imageName retornado
        await _repo.Received(1).UpdateAsync(
            Arg.Is<Producto>(p => p.ImagePath == "guardada/camaro_001.jpg"));
    }

    // ── Guard: ImagePath null → NO llama SaveImageAsync ──────────────────────

    [Fact]
    public async Task Handle_ImagePathNull_NoLlamaSaveImageAsync()
    {
        // Mata el mutante: ImagePath is not null → true (siempre guarda)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 25m, 5, null),
            CancellationToken.None);

        await _imageStorage.DidNotReceive().SaveImageAsync(Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_ImagePathNoNull_LlamaSaveImageAsync()
    {
        // Par complementario: mata el mutante ImagePath is not null → false (nunca guarda)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _imageStorage.SaveImageAsync(Arg.Any<string?>()).Returns("guardada.jpg");
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 25m, 5, "origen.jpg"),
            CancellationToken.None);

        await _imageStorage.Received(1).SaveImageAsync("origen.jpg");
    }

    // ── Guard: producto is null ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoNoExiste_RetornaFailureConMensaje()
    {
        // Mata el mutante: producto is null → producto is not null
        _repo.GetByIdAsync(99).Returns((Producto?)null);

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(99, "Test", 10m, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("El producto no existe.");
    }

    [Fact]
    public async Task Handle_ProductoNoExiste_NoLlamaUpdateAsync()
    {
        // Mata el mutante que elimina el return temprano ante null
        _repo.GetByIdAsync(99).Returns((Producto?)null);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(99, "Test", 10m, 1, null),
            CancellationToken.None);

        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Producto>());
    }

    // ── ActualizarDatos: parámetros pasados correctamente ────────────────────

    [Fact]
    public async Task Handle_ActualizarDatos_PasaParametrosCorrectamente()
    {
        // Mata mutantes en new NombreProducto(request.Nombre),
        // new Precio(request.Precio), new Unidades(request.Unidades)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        Producto? capturado = null;
        _repo.UpdateAsync(Arg.Do<Producto>(p => capturado = p))
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Nombre Nuevo", 99.99m, 42, null),
            CancellationToken.None);

        capturado!.Nombre.Value.Should().Be("Nombre Nuevo");
        capturado.Precio.Value.Should().Be(99.99m);
        capturado.Stock.Value.Should().Be(42);
    }

    // ── Catch: ArgumentException ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_NombreInvalido_RetornaFailurePorArgumentException()
    {
        // Nombre vacío lanza ArgumentException en new NombreProducto("")
        // capturado por catch(ArgumentException ex) → Result.Failure(ex.Message)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Producto>());
    }

    [Fact]
    public async Task Handle_PrecioNegativo_RetornaFailurePorArgumentException()
    {
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Test", -1m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── Catch: Exception genérica ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionEnGetById_RetornaFailure()
    {
        _repo.When(x => x.GetByIdAsync(Arg.Any<int>()))
             .Throw(new Exception("Error de conexión"));

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Test", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_ExcepcionEnUpdateAsync_RetornaFailure()
    {
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _repo.When(x => x.UpdateAsync(Arg.Any<Producto>()))
             .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

// ═══════════════════════════════════════════════════════════════════════════
// ActualizarProductoCommandHandler — survivors
// Constructor: (IProductoRepository, IImageStorageService, IAppLogger)
// ═══════════════════════════════════════════════════════════════════════════


    // ── ImagePath is not null — par ambas ramas ───────────────────────────────

    [Fact]
    public async Task Handle_ImagePathNoNull_LlamaSaveImageAsyncYAsignaPath()
    {
        // Mata mutante: ImagePath is not null → false (nunca guarda)
        _repo.GetByIdAsync(1).Returns(ProductoExistente());
        _imageStorage.SaveImageAsync("origen/camaro.jpg").Returns("guardada/camaro_001.jpg");
        _repo.UpdateAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Camaro", 30m, 5, "origen/camaro.jpg"),
            CancellationToken.None);

        await _imageStorage.Received(1).SaveImageAsync("origen/camaro.jpg");
        await _repo.Received(1).UpdateAsync(
            Arg.Is<Producto>(p => p.ImagePath == "guardada/camaro_001.jpg"));
    }

    // ── producto is null ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoNoExiste_RetornaFailureSinLlamarUpdate()
    {
        _repo.GetByIdAsync(99).Returns((Producto?)null);

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(99, "Test", 10m, 1, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("El producto no existe.");
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Producto>());
    }

    // ── ActualizarDatos: parámetros correctos ─────────────────────────────────

    [Fact]
    public async Task Handle_ActualizarDatos_PasaParametrosCorrectos()
    {
        // Mata mutantes: Nombre, Precio, Unidades pasados con valores incorrectos
        _repo.GetByIdAsync(1).Returns(ProductoExistente());

        Producto? capturado = null;
        _repo.UpdateAsync(Arg.Do<Producto>(p => capturado = p)).Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Nombre Nuevo", 99.99m, 42, null),
            CancellationToken.None);

        capturado!.Nombre.Value.Should().Be("Nombre Nuevo");
        capturado.Precio.Value.Should().Be(99.99m);
        capturado.Stock.Value.Should().Be(42);
    }

    // ── catch blocks ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionGenerica_RetornaFailureConMensajeFijo()
    {
        _repo.When(x => x.GetByIdAsync(Arg.Any<int>()))
             .Throw(new Exception("Error de red"));

        var result = await CrearHandler().Handle(
            new ActualizarProductoCommand(1, "Test", 25m, 5, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("guardar el producto");
    }
}
