using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.CrearProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
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
    private readonly IProductoRepository _repo;
    private readonly CrearProductoCommandHandler _handler;

    public CrearProductoCommandHandlerTests()
    {
        _repo    = Substitute.For<IProductoRepository>();
        _handler = new CrearProductoCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_CommandHotWheelsValido_DebeGuardarYRetornarSuccess()
    {
        _repo.AddAsync(Arg.Any<Producto>()).Returns(Task.FromResult(1));

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
        await _repo.Received(1).AddAsync(Arg.Any<Producto>());
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
        await _repo.DidNotReceive().AddAsync(Arg.Any<Producto>());
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
        await _repo.DidNotReceive().AddAsync(Arg.Any<Producto>());
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
        _handler = new BuscarProductosQueryHandler(_repo);
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