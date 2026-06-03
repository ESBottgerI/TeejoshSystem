using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class VentaDetalleTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static VentaDetalle DetalleValido(
        int productoId = 1,
        string nombre = "Pikachu V",
        int cantidad = 2,
        decimal precio = 25.00m)
        => new(productoId, nombre, cantidad, precio);

    // ═════════════════════════════════════════════════════════════════════════
    // VentaDetalle — construcción válida
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void VentaDetalle_Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        var detalle = new VentaDetalle(1, "Pikachu V", 2, 25.00m);

        detalle.ProductoId.Should().Be(1);
        detalle.NombreProducto.Should().Be("Pikachu V");
        detalle.Cantidad.Should().Be(2);
        detalle.PrecioUnitario.Should().Be(25.00m);
    }

    [Fact]
    public void VentaDetalle_Constructor_ConPrecioEnCero_DebeCrearInstancia()
    {
        // precioUnitario == 0 es válido: la regla solo prohíbe negativos
        var act = () => new VentaDetalle(1, "Producto Gratis", 1, 0m);

        act.Should().NotThrow();
    }

    [Fact]
    public void VentaDetalle_Constructor_ConCantidadUno_DebeCrearInstancia()
    {
        // Boundary inferior válido: cantidad == 1
        var act = () => new VentaDetalle(1, "Producto", 1, 10m);

        act.Should().NotThrow();
    }

    // ── VentaDetalle — cantidad inválida (mutante <= vs <) ───────────────────

    [Fact]
    public void VentaDetalle_Constructor_ConCantidadCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 debe fallar (cantidad <= 0 es el guard)
        var act = () => new VentaDetalle(1, "Producto", 0, 10m);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*mayor a cero*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(int.MinValue)]
    public void VentaDetalle_Constructor_ConCantidadNegativa_DebeArrojarArgumentException(int cantidad)
    {
        var act = () => new VentaDetalle(1, "Producto", cantidad, 10m);

        act.Should().Throw<ArgumentException>();
    }

    // ── VentaDetalle — precio inválido (mutante < vs <=) ─────────────────────

    [Fact]
    public void VentaDetalle_Constructor_ConPrecioNegativo_DebeArrojarArgumentException()
    {
        // -0.01 es el caso boundary que mata el mutante < → <=
        var act = () => new VentaDetalle(1, "Producto", 1, -0.01m);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*negativo*");
    }

    [Theory]
    [InlineData(-1.00)]
    [InlineData(-99.99)]
    public void VentaDetalle_Constructor_ConPrecioMuyNegativo_DebeArrojarArgumentException(decimal precio)
    {
        var act = () => new VentaDetalle(1, "Producto", 1, precio);

        act.Should().Throw<ArgumentException>();
    }

    // ── VentaDetalle — nombre inválido ────────────────────────────────────────

    [Fact]
    public void VentaDetalle_Constructor_ConNombreNull_DebeArrojarArgumentException()
    {
        var act = () => new VentaDetalle(1, null!, 1, 10m);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*obligatorio*");
    }

    [Fact]
    public void VentaDetalle_Constructor_ConNombreVacio_DebeArrojarArgumentException()
    {
        var act = () => new VentaDetalle(1, string.Empty, 1, 10m);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    public void VentaDetalle_Constructor_ConNombreSoloEspacios_DebeArrojarArgumentException(string nombre)
    {
        var act = () => new VentaDetalle(1, nombre, 1, 10m);

        act.Should().Throw<ArgumentException>();
    }

    // ── VentaDetalle — Subtotal (mata mutante en operador *) ─────────────────

    [Fact]
    public void VentaDetalle_Subtotal_EsCantidadPorPrecioUnitario()
    {
        var detalle = new VentaDetalle(1, "Charizard EX", 3, 80.00m);

        // 3 * 80.00 = 240.00
        detalle.Subtotal.Should().Be(240.00m);
    }

    [Fact]
    public void VentaDetalle_Subtotal_ConCantidadUnoDevuelvePrecioUnitario()
    {
        // Caso degenerado: cantidad = 1 → subtotal = precioUnitario
        var detalle = new VentaDetalle(1, "Producto", 1, 99.99m);

        detalle.Subtotal.Should().Be(99.99m);
    }

    [Fact]
    public void VentaDetalle_Subtotal_ConPrecioEnCeroDevuelveCero()
    {
        var detalle = new VentaDetalle(1, "Promo", 5, 0m);

        detalle.Subtotal.Should().Be(0m);
    }

    // ── VentaDetalle — snapshot de precio ────────────────────────────────────

    [Fact]
    public void VentaDetalle_PrecioUnitario_EsInmutableDespuesDeConstruccion()
    {
        // La invariante más crítica: el precio es un snapshot histórico
        var precioOriginal = 25.00m;
        var detalle = new VentaDetalle(1, "Pikachu V", 2, precioOriginal);

        // No hay setter — la inmutabilidad la garantiza el compilador.
        // Este test documenta la intención y valida que el valor se preserva.
        detalle.PrecioUnitario.Should().Be(precioOriginal);
    }

    [Fact]
    public void VentaDetalle_NombreProducto_EsSnapshotHistorico()
    {
        var nombreOriginal = "Pikachu V Full Art";
        var detalle = new VentaDetalle(1, nombreOriginal, 1, 50m);

        detalle.NombreProducto.Should().Be(nombreOriginal);
    }

    // ── Venta — AgregarDetalle (mata mutante en Total +=) ────────────────────

    [Fact]
    public void Venta_AgregarDetalle_UnDetalle_TotalEsSubtotalDelDetalle()
    {
        var venta = new Venta(DateTime.UtcNow);
        var detalle = new VentaDetalle(1, "Pikachu V", 2, 25.00m); // subtotal = 50

        venta.AgregarDetalle(detalle);

        // Mata el mutante Total += detalle.Subtotal → Total = detalle.Subtotal
        venta.Total.Should().Be(50.00m);
    }

    [Fact]
    public void Venta_AgregarDetalle_DosDetalles_TotalAcumulaAmbos()
    {
        // Este test mata el mutante de reemplazo (Total = X en lugar de Total += X)
        var venta = new Venta(DateTime.UtcNow);

        venta.AgregarDetalle(new VentaDetalle(1, "Pikachu V", 2, 25.00m)); // 50
        venta.AgregarDetalle(new VentaDetalle(2, "Charizard EX", 1, 80.00m)); // 80

        venta.Total.Should().Be(130.00m);
    }

    [Fact]
    public void Venta_AgregarDetalle_TresDetalles_TotalEsLaSumaCorrecta()
    {
        var venta = new Venta(DateTime.UtcNow);

        venta.AgregarDetalle(new VentaDetalle(1, "Pikachu V", 2, 25.00m)); // 50
        venta.AgregarDetalle(new VentaDetalle(2, "Charizard EX", 1, 80.00m)); // 80
        venta.AgregarDetalle(new VentaDetalle(3, "Dragon Blanco", 3, 10.00m)); // 30

        venta.Total.Should().Be(160.00m);
    }

    [Fact]
    public void Venta_AgregarDetalle_AumentaConteoDeDetalles()
    {
        var venta = new Venta(DateTime.UtcNow);

        venta.AgregarDetalle(DetalleValido(1, "A"));
        venta.AgregarDetalle(DetalleValido(2, "B"));

        venta.Detalles.Should().HaveCount(2);
    }

    [Fact]
    public void Venta_AgregarDetalle_ConNull_DebeArrojarArgumentNullException()
    {
        var venta = new Venta(DateTime.UtcNow);

        var act = () => venta.AgregarDetalle(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Venta — IReadOnlyList protege la lista interna ────────────────────────

    [Fact]
    public void Venta_Detalles_EsReadOnly_NoExponeLista()
    {
        var venta = new Venta(DateTime.UtcNow);
        venta.AgregarDetalle(DetalleValido());

        // La propiedad Detalles retorna IReadOnlyList, no List<T>
        venta.Detalles.Should().BeAssignableTo<IReadOnlyList<VentaDetalle>>();
    }

    [Fact]
    public void Venta_Detalles_ContieneElDetalleAgregado()
    {
        var venta = new Venta(DateTime.UtcNow);
        var detalle = DetalleValido();

        venta.AgregarDetalle(detalle);

        venta.Detalles.Should().ContainSingle()
             .Which.Should().BeSameAs(detalle);
    }
}
