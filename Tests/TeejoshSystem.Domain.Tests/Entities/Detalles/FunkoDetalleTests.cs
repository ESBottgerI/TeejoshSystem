using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class FunkoDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // FunkoDetalle
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Funko_Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        var detalle = new FunkoDetalle(1138, "Batman DC", 2, null);

        detalle.NumeroCaja.Should().Be(1138);
        detalle.Licencia.Should().Be("Batman DC");
        detalle.SubtipoId.Should().Be(2);
        detalle.CaracteristicaEspecialId.Should().BeNull();
    }

    [Fact]
    public void Funko_Constructor_ConCaracteristicaEspecialId_DebeAsignarValor()
    {
        var detalle = new FunkoDetalle(500, "Naruto", 1, 3);

        detalle.CaracteristicaEspecialId.Should().Be(3);
    }

    // ── NumeroCaja — boundaries ───────────────────────────────────────────────

    [Fact]
    public void Funko_Constructor_ConNumeroCajaUno_DebeCrearInstancia()
    {
        // Boundary inferior válido: 1
        var act = () => new FunkoDetalle(1, "Licencia", 1, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Funko_Constructor_ConNumeroCajaCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 inválido — mata mutante <= → <
        var act = () => new FunkoDetalle(0, "Licencia", 1, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalido*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Funko_Constructor_ConNumeroCajaNegativo_DebeArrojarArgumentException(int numero)
    {
        var act = () => new FunkoDetalle(numero, "Licencia", 1, null);

        act.Should().Throw<ArgumentException>();
    }

    // ── Licencia — IsNullOrWhiteSpace ─────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Funko_Constructor_ConLicenciaInvalida_DebeArrojarArgumentException(string? licencia)
    {
        var act = () => new FunkoDetalle(100, licencia!, 1, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*licencia*");
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void Funko_Actualizar_ConDatosValidos_DebeActualizarPropiedades()
    {
        var detalle = new FunkoDetalle(100, "Batman DC", 1, null);

        detalle.Actualizar(200, "Spider-Man Marvel", 2, 5);

        detalle.NumeroCaja.Should().Be(200);
        detalle.Licencia.Should().Be("Spider-Man Marvel");
        detalle.SubtipoId.Should().Be(2);
        detalle.CaracteristicaEspecialId.Should().Be(5);
    }

    [Fact]
    public void Funko_Actualizar_ConNumeroCajaInvalido_DebeArrojar()
    {
        var detalle = new FunkoDetalle(100, "Licencia", 1, null);

        var act = () => detalle.Actualizar(0, "Licencia", 1, null);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: licencia inválida (los 2 "no cov" son null y whitespace) ──

    [Fact]
    public void Actualizar_LicenciaNull_DebeArrojar()
    {
        // Mata el mutante IsNullOrWhiteSpace(licencia) en Actualizar (no cov)
        var detalle = new FunkoDetalle(100, "Licencia", 1, null);

        var act = () => detalle.Actualizar(100, null!, 1, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*licencia*");
    }

    [Fact]
    public void Actualizar_LicenciaVacia_DebeArrojar()
    {
        var detalle = new FunkoDetalle(100, "Licencia", 1, null);

        var act = () => detalle.Actualizar(100, "", 1, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_LicenciaSoloEspacios_DebeArrojar()
    {
        // Mata el tercer mutante residual de IsNullOrWhiteSpace en Actualizar
        var detalle = new FunkoDetalle(100, "Licencia", 1, null);

        var act = () => detalle.Actualizar(100, "   ", 1, null);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: NumeroCaja boundary (para completar cobertura de Actualizar)

    [Fact]
    public void Actualizar_NumeroCajaCero_DebeArrojar()
    {
        var detalle = new FunkoDetalle(100, "Licencia", 1, null);

        var act = () => detalle.Actualizar(0, "Licencia", 1, null);

        act.Should().Throw<ArgumentException>();
    }
}
