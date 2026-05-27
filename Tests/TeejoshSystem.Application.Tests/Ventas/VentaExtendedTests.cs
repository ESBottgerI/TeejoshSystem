using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Commands.RegistrarVenta;
using TeejoshSystem.Application.Ports.Inbound.Ventas.Queries.ObtenerVentas;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ventas;

// ═══════════════════════════════════════════════════════════════════════════
// RegistrarVentaCommand — constructor
//
// Mutantes objetivo:
//   - items is null || items.Count == 0 → boundary lista vacía vs nula
//   - operador || → && (mutante clásico en OR compuesto)
// ═══════════════════════════════════════════════════════════════════════════

public class RegistrarVentaCommandTests
{
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
}

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerVentasQueryHandler — extensión de casos existentes
//
// Mutantes objetivo:
//   - OrderByDescending(v => v.Fecha) → sin orden o ascendente
//   - Select mapping: Id, Fecha, Total, Detalles
//   - Detalles: cada campo del VentaDetalleDto
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerVentasQueryHandlerExtendedTests
{
    private readonly IVentaRepository _ventaRepo = Substitute.For<IVentaRepository>();
    private readonly ObtenerVentasQueryHandler _handler;

    public ObtenerVentasQueryHandlerExtendedTests()
        => _handler = new ObtenerVentasQueryHandler(_ventaRepo);

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