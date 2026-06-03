using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities;

public class VentaTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // Venta — construcción
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Venta_Constructor_ConFechaValida_DebeInicializarTotalEnCero()
    {
        var fecha = new DateTime(2026, 5, 10);
        var venta = new Venta(fecha);

        venta.Fecha.Should().Be(fecha);
        venta.Total.Should().Be(0m);
    }

    [Fact]
    public void Venta_Constructor_DeberiaInicializarDetallesVacios()
    {
        var venta = new Venta(DateTime.UtcNow);

        venta.Detalles.Should().BeEmpty();
    }

    // ── Venta — Total nunca puede quedar inconsistente ────────────────────────

    [Fact]
    public void Venta_TotalInicial_EsCero_AntesDeCualquierDetalle()
    {
        var venta = new Venta(DateTime.UtcNow);

        venta.Total.Should().Be(0m);
        venta.Detalles.Should().BeEmpty();
    }

    [Fact]
    public void Venta_Total_SeCorrespondeConSubtotalAcumulado()
    {
        var venta = new Venta(DateTime.UtcNow);
        var detalle1 = new VentaDetalle(1, "Producto A", 3, 10.00m); // 30
        var detalle2 = new VentaDetalle(2, "Producto B", 2, 15.00m); // 30

        venta.AgregarDetalle(detalle1);
        venta.AgregarDetalle(detalle2);

        var subtotalCalculado = venta.Detalles.Sum(d => d.Subtotal);
        venta.Total.Should().Be(subtotalCalculado);
    }
}