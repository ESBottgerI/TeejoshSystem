using FluentAssertions;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Productos;

/// <summary>
/// Tests de los handlers de Productos.
/// El repositorio se mockea con NSubstitute — no hay base de datos real.
///
/// AJUSTE REQUERIDO: si los namespaces de tus handlers difieren,
/// actualiza los using correspondientes.
/// </summary>
public class CrearProductoCommandHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly CrearProductoCommandHandler _handler;

    public CrearProductoCommandHandlerTests()
    {
        _repo = Substitute.For<IProductoRepository>();
        _handler = new CrearProductoCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_CommandValido_DebeGuardarYRetornarSuccess()
    {
        _repo.AddAsync(Arg.Any<Producto>()).Returns(Task.CompletedTask);

        var command = new CrearProductoCommand
        {
            Tipo    = TipoProducto.HotWheels,
            Nombre  = "Camaro 1969",
            Precio  = 25.00m,
            Stock   = 3
            // Ajustar campos del detalle según el DTO de tu comando
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Any<Producto>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public async Task Handle_NombreInvalido_DebeRetornarFailureSinPersistir(string? nombre)
    {
        var command = new CrearProductoCommand
        {
            Tipo   = TipoProducto.Funko,
            Nombre = nombre!,
            Precio = 15m,
            Stock  = 1
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
            Tipo   = TipoProducto.Toy,
            Nombre = "Test",
            Precio = -5m,
            Stock  = 1
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().AddAsync(Arg.Any<Producto>());
    }
}

public class ObtenerProductosQueryHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly ObtenerProductosQueryHandler _handler;

    public ObtenerProductosQueryHandlerTests()
    {
        _repo = Substitute.For<IProductoRepository>();
        _handler = new ObtenerProductosQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ExistenProductos_DebeRetornarDtosDeLosMismos()
    {
        var productos = new List<Producto>
        {
            FabricarProducto("Supra MK4",   TipoProducto.HotWheels),
            FabricarProducto("Pikachu 25°", TipoProducto.Funko)
        };
        _repo.GetAllAsync().Returns(productos);

        var result = await _handler.Handle(new ObtenerProductosQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_SinProductos_DebeRetornarListaVaciaConSuccess()
    {
        _repo.GetAllAsync().Returns(new List<Producto>());

        var result = await _handler.Handle(new ObtenerProductosQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    private static Producto FabricarProducto(string nombre, TipoProducto tipo)
        => new(tipo, new NombreProducto(nombre), new Precio(10m), new Unidades(1));
}

public class EliminarProductoCommandHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly EliminarProductoCommandHandler _handler;

    public EliminarProductoCommandHandlerTests()
    {
        _repo = Substitute.For<IProductoRepository>();
        _handler = new EliminarProductoCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ProductoExistente_DebeEliminarYRetornarSuccess()
    {
        var producto = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Test"),
            new Precio(10m),
            new Unidades(1));

        _repo.GetByIdAsync(1).Returns(producto);
        _repo.DeleteAsync(1).Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new EliminarProductoCommand { Id = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).DeleteAsync(1);
    }

    [Fact]
    public async Task Handle_ProductoInexistente_DebeRetornarFailureSinEliminar()
    {
        _repo.GetByIdAsync(99).Returns((Producto?)null);

        var result = await _handler.Handle(
            new EliminarProductoCommand { Id = 99 }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }
}

public class BuscarProductosQueryHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly BuscarProductosQueryHandler _handler;

    public BuscarProductosQueryHandlerTests()
    {
        _repo = Substitute.For<IProductoRepository>();
        _handler = new BuscarProductosQueryHandler(_repo);
    }

    [Fact]
    public async Task Handle_ConTerminoYTipo_DebeDelegarAlRepositorioConAmbosParametros()
    {
        var esperados = new List<ProductoBusquedaResult>(); // vacío es suficiente
        _repo.SearchAsync("Ford", TipoProducto.HotWheels).Returns(esperados);

        var result = await _handler.Handle(
            new BuscarProductosQuery { Termino = "Ford", Tipo = TipoProducto.HotWheels },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).SearchAsync("Ford", TipoProducto.HotWheels);
    }
}
