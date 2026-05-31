using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.ActualizarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Productos;

// ═══════════════════════════════════════════════════════════════════════════
// CrearProductoCommandHandler
// Retorna Result (no Result<T>)
// El detalle se pasa en propiedades anidadas (HotWheels, Funko, etc.)
// ═══════════════════════════════════════════════════════════════════════════

public class CrearProductoCommandHandlerTests
{
    private readonly IProductoRepository _repository;

    private readonly CrearProductoCommandHandler _handler;

    private readonly IImageStorageService _imageStorageMock;

    private readonly IAppLogger _applogger;

    public CrearProductoCommandHandlerTests()
    {
        _repository = Substitute.For<IProductoRepository>();
        
        _imageStorageMock = Substitute.For<IImageStorageService>();

        _applogger = Substitute.For<IAppLogger>();

        _handler = new CrearProductoCommandHandler(
            _repository,
            _imageStorageMock,
            _applogger
        );

    }

    [Fact]
    public async Task Handle_CommandHotWheelsValido_DebeGuardarYRetornarSuccess()
    {
        _repository.AddAsync(Arg.Any<Producto>()).Returns(Task.FromResult(1));

        var command = new CrearProductoCommand
        {
            Tipo     = TipoProducto.HotWheels,
            Nombre   = "Camaro 1969",
            Precio   = 25m,
            Unidades = 3,
            HotWheels = new CrearHotWheelsDetalleDto("Camaro", 2020, "Muscle Mania", 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<Producto>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreInvalido_DebeRetornarFailureSinPersistir(string nombre)
    {
        var command = new CrearProductoCommand
        {
            Tipo     = TipoProducto.HotWheels,
            Nombre   = nombre,
            Precio   = 15m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Camaro", 2020, "Test", 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Producto>());
    }

    [Fact]
    public async Task Handle_PrecioNegativo_DebeRetornarFailureSinPersistir()
    {
        var command = new CrearProductoCommand
        {
            Tipo     = TipoProducto.HotWheels,
            Nombre   = "Test",
            Precio   = -5m,
            Unidades = 1,
            HotWheels = new CrearHotWheelsDetalleDto("Camaro", 2020, "Test", 1)
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Producto>());
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerProductosQueryHandler
// Retorna IReadOnlyList<ProductoDto> directamente — sin Result wrapper
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerProductosQueryHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly ObtenerProductosQueryHandler _handler;

    public ObtenerProductosQueryHandlerTests()
    {
        _repo    = Substitute.For<IProductoRepository>();
        _handler = new ObtenerProductosQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ExistenProductos_DebeRetornarUnDtoPorProducto()
    {
        var productos = new List<Producto>
        {
            new(TipoProducto.HotWheels, new NombreProducto("Supra MK4"),   new Precio(10m), new Unidades(1)),
            new(TipoProducto.Funko,     new NombreProducto("Pikachu 25°"), new Precio(15m), new Unidades(2))
        };
        _repo.GetAllAsync().Returns(productos);

        // Retorna IReadOnlyList<ProductoDto>, no Result<>
        var result = await _handler.Handle(new ObtenerProductosQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(d => d.Nombre == "Supra MK4");
    }

    [Fact]
    public async Task Handle_SinProductos_DebeRetornarListaVacia()
    {
        _repo.GetAllAsync().Returns(new List<Producto>());

        var result = await _handler.Handle(new ObtenerProductosQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// EliminarProductoCommandHandler
// DeleteAsync recibe Producto, no int
// Command acepta List<int>
// ═══════════════════════════════════════════════════════════════════════════

public class EliminarProductoCommandHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly EliminarProductoCommandHandler _handler;

    public EliminarProductoCommandHandlerTests()
    {
        _repo    = Substitute.For<IProductoRepository>();
        _handler = new EliminarProductoCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_IdsValidos_DebeDelgarADeleteRangeYRetornarSuccess()
    {
        _repo.DeleteRangeAsync(Arg.Any<IEnumerable<int>>())
             .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new EliminarProductoCommand(new List<int> { 1, 2 }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1)
                   .DeleteRangeAsync(Arg.Is<IEnumerable<int>>(ids =>
                       ids.SequenceEqual(new[] { 1, 2 })));
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DebeRetornarFailure()
    {
        // NSubstitute: When(...).Throw(...) para simular excepciones en async
        _repo.When(x => x.DeleteRangeAsync(Arg.Any<IEnumerable<int>>()))
             .Throw(new Exception("Error de BD"));

        var result = await _handler.Handle(
            new EliminarProductoCommand(new List<int> { 99 }),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// BuscarProductosQueryHandler
// Retorna IReadOnlyList<ProductoBusquedaDto> directamente (sin Result)
// ═══════════════════════════════════════════════════════════════════════════

public class BuscarProductosQueryHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly BuscarProductosQueryHandler _handler;

    public BuscarProductosQueryHandlerTests()
    {
        _repo    = Substitute.For<IProductoRepository>();
        
        var imageStorage = Substitute.For<IImageStorageService>();

        _handler = new BuscarProductosQueryHandler(
            _repo,
            imageStorage
        );
    }

    [Fact]
    public async Task Handle_ConTerminoYTipo_DebeDelegarConAmbosParametros()
    {
        _repo.SearchWithDetalleAsync("Ford", TipoProducto.HotWheels)
             .Returns(new List<ProductoBusquedaResult>());

        var result = await _handler.Handle(
            new BuscarProductosQuery("Ford", TipoProducto.HotWheels),
            CancellationToken.None);

        result.Should().NotBeNull();
        await _repo.Received(1).SearchWithDetalleAsync("Ford", TipoProducto.HotWheels);
    }

    [Fact]
    public async Task Handle_SinFiltros_DebeLlamarConNulos()
    {
        _repo.SearchWithDetalleAsync(null, null)
             .Returns(new List<ProductoBusquedaResult>());

        var result = await _handler.Handle(
            new BuscarProductosQuery(null, null),
            CancellationToken.None);

        result.Should().BeEmpty();
        await _repo.Received(1).SearchWithDetalleAsync(null, null);
    }
}

/// <summary>
/// Tests de ActualizarProductoCommandHandler.
///
/// Mutantes objetivo:
///   - producto is null       → invertido (skippea null check)
///   - ImagePath is not null  → invertido (siempre/nunca guarda imagen)
///   - SaveImageAsync(request.ImagePath)  → arg incorrecto
///   - AsignarImagePath(imageName)        → arg incorrecto
///   - UpdateAsync siempre invocado       → eliminado
///   - ActualizarDatos params             → arg mutado
///   - catch blocks                       → Failure vs re-throw
///   - Result.Success/Failure             → intercambiados
/// </summary>
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
}