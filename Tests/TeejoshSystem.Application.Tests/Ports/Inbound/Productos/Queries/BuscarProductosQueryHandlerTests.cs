using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Queries;

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
        _repo = Substitute.For<IProductoRepository>();

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
