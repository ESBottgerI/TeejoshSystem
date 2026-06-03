using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Queries;

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
        _repo = Substitute.For<IProductoRepository>();
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

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerProductosQueryHandler — survivors (3 survived)
//
// Los tests existentes solo afirman Nombre y Count.
// Mutantes objetivo: Id, Tipo, Precio, Unidades, TipoDescripcion, DetalleResumen
// ═══════════════════════════════════════════════════════════════════════════

    private ObtenerProductosQueryHandler CrearHandler() => new(_repo);

    private static Producto CrearProductoConId(int id, TipoProducto tipo, string nombre, decimal precio, int stock)
    {
        var p = new Producto(tipo, new NombreProducto(nombre), new Precio(precio), new Unidades(stock));
        typeof(Producto).GetProperty("Id")!.SetValue(p, id);
        return p;
    }

    [Fact]
    public async Task Handle_MapeaTodosLosCamposDelProductoDto()
    {
        // Un solo test mata todos los mutantes de la projection:
        // Id → 0,  Tipo → default,  Precio → 0,  Unidades → 0,
        // TipoDescripcion → null,  DetalleResumen → algo distinto de ""
        var producto = CrearProductoConId(42, TipoProducto.Funko, "Pikachu 25°", 15.50m, 7);
        _repo.GetAllAsync().Returns(new List<Producto> { producto });

        var result = await CrearHandler().Handle(new ObtenerProductosQuery(), CancellationToken.None);

        var dto = result.Single();
        dto.Id.Should().Be(42);
        dto.Tipo.Should().Be(TipoProducto.Funko);
        dto.Nombre.Should().Be("Pikachu 25°");
        dto.Precio.Should().Be(15.50m);
        dto.Unidades.Should().Be(7);
        dto.TipoDescripcion.Should().Be(TipoProducto.Funko.ToString());
        dto.DetalleResumen.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_TipoDescripcion_EsToStringDelTipo()
    {
        // Mata mutante: TipoDescripcion = p.Tipo.ToString() → null o constante
        var hw = CrearProductoConId(1, TipoProducto.HotWheels, "Supra", 10m, 1);
        var toy = CrearProductoConId(2, TipoProducto.Toy, "Puzzle", 20m, 1);
        _repo.GetAllAsync().Returns(new List<Producto> { hw, toy });

        var result = await CrearHandler().Handle(new ObtenerProductosQuery(), CancellationToken.None);

        result.Single(d => d.Id == 1).TipoDescripcion.Should().Be("HotWheels");
        result.Single(d => d.Id == 2).TipoDescripcion.Should().Be("Toy");
    }
}

