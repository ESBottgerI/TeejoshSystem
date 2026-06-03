using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries.ObtenerVentas;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Ventas.Queries;

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerVentasQueryHandler
// Retorna IReadOnlyList<VentaDto> directamente — sin Result wrapper.
// IVentaRepository expone GetByFechaAsync(DateTime?, DateTime?), no GetAllAsync.
// Venta(DateTime fecha) — constructor con parámetro requerido.
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerVentasQueryHandlerTests
{
    private readonly IVentaRepository _ventaRepo;
    private readonly ObtenerVentasQueryHandler _handler;

    public ObtenerVentasQueryHandlerTests()
    {
        _ventaRepo = Substitute.For<IVentaRepository>();
        _handler = new ObtenerVentasQueryHandler(_ventaRepo);
    }

    [Fact]
    public async Task Handle_ExistenVentas_DebeRetornarDtosOrdenadosPorFechaDesc()
    {
        var hoy = DateTime.Today;
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

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerVentasQueryHandler — extensión de casos existentes
//
// Mutantes objetivo:
//   - OrderByDescending(v => v.Fecha) → sin orden o ascendente
//   - Select mapping: Id, Fecha, Total, Detalles
//   - Detalles: cada campo del VentaDetalleDto
// ═══════════════════════════════════════════════════════════════════════════

    private static Venta FabricarVentaConDetalle(DateTime fecha, decimal precio = 25m)
    {
        var venta = new Venta(fecha);
        venta.AgregarDetalle(new Domain.Entities.Detalles.VentaDetalle(1, "Pikachu V", 2, precio));
        return venta;
    }

    // ── Orden descendente — mutante OrderByDescending → OrderBy ──────────────

    [Fact]
    public async Task Handle_TresVentas_OrdenadaPorFechaDescendente()
    {
        var v1 = new Venta(new DateTime(2026, 5, 1));
        var v2 = new Venta(new DateTime(2026, 5, 3)); // más reciente
        var v3 = new Venta(new DateTime(2026, 5, 2));

        _ventaRepo.GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                  .Returns(new List<Venta> { v1, v2, v3 });

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        // El orden correcto es: 3-may, 2-may, 1-may
        result[0].Fecha.Should().Be(new DateTime(2026, 5, 3));
        result[1].Fecha.Should().Be(new DateTime(2026, 5, 2));
        result[2].Fecha.Should().Be(new DateTime(2026, 5, 1));
    }

    // ── Mapping: VentaDto campos ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_MapeoVentaDto_FechaYTotalCorrectos()
    {
        var fecha = new DateTime(2026, 5, 10, 14, 30, 0);
        var venta = FabricarVentaConDetalle(fecha, 80m);

        typeof(Venta).GetProperty("Id")!.SetValue(venta, 99);

        _ventaRepo.GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                  .Returns(new List<Venta> { venta });

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        var dto = result.Single();
        dto.Fecha.Should().Be(fecha);
        dto.Total.Should().Be(160m); // 2 * 80
    }

    // ── Mapping: VentaDetalleDto campos ──────────────────────────────────────

    [Fact]
    public async Task Handle_MapeoDetalleDto_CamposCorrectos()
    {
        // Mata mutantes que cambian ProductoId, NombreProducto, Cantidad, PrecioUnitario o Subtotal
        var venta = new Venta(DateTime.Today);
        venta.AgregarDetalle(new Domain.Entities.Detalles.VentaDetalle(42, "Charizard EX", 3, 80m));

        _ventaRepo.GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                  .Returns(new List<Venta> { venta });

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        var detalle = result.Single().Detalles.Single();
        detalle.ProductoId.Should().Be(42);
        detalle.NombreProducto.Should().Be("Charizard EX");
        detalle.Cantidad.Should().Be(3);
        detalle.PrecioUnitario.Should().Be(80m);
        detalle.Subtotal.Should().Be(240m); // 3 * 80
    }

    [Fact]
    public async Task Handle_ConFiltrosDeFecha_PasaParametrosAlRepositorio()
    {
        var desde = new DateTime(2026, 5, 1);
        var hasta = new DateTime(2026, 5, 31);

        _ventaRepo.GetByFechaAsync(desde, hasta).Returns(new List<Venta>());

        await _handler.Handle(new ObtenerVentasQuery(desde, hasta), CancellationToken.None);

        await _ventaRepo.Received(1).GetByFechaAsync(desde, hasta);
    }

    [Fact]
    public async Task Handle_SinFiltros_PasaNulosAlRepositorio()
    {
        _ventaRepo.GetByFechaAsync(null, null).Returns(new List<Venta>());

        await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        await _ventaRepo.Received(1).GetByFechaAsync(null, null);
    }

    [Fact]
    public async Task Handle_VentaConMultiplesDetalles_MapeaTodos()
    {
        var venta = new Venta(DateTime.Today);
        venta.AgregarDetalle(new Domain.Entities.Detalles.VentaDetalle(1, "Producto A", 1, 10m));
        venta.AgregarDetalle(new Domain.Entities.Detalles.VentaDetalle(2, "Producto B", 2, 20m));

        _ventaRepo.GetByFechaAsync(Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
                  .Returns(new List<Venta> { venta });

        var result = await _handler.Handle(new ObtenerVentasQuery(), CancellationToken.None);

        result.Single().Detalles.Should().HaveCount(2);
    }
}