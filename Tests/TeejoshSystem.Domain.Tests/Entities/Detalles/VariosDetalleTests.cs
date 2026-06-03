using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class VariosDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // VariosDetalle
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Varios_Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        var detalle = new VariosDetalle("Bandai", 15m, 10m, 5m, "Plástico", true);

        detalle.Marca.Should().Be("Bandai");
        detalle.Alto.Should().Be(15m);
        detalle.Ancho.Should().Be(10m);
        detalle.Largo.Should().Be(5m);
        detalle.Material.Should().Be("Plástico");
        detalle.TieneIlustracion.Should().BeTrue();
    }

    [Fact]
    public void Varios_Constructor_ConLargoNull_DebeCrearInstancia()
    {
        // Largo es nullable — null es válido
        var act = () => new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Varios_Constructor_LargoNull_DebeQuedarNull()
    {
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        detalle.Largo.Should().BeNull();
    }

    // ── Alto > 0 (boundaries) ────────────────────────────────────────────────

    [Fact]
    public void Varios_Constructor_ConAltoMinimo_DebeCrearInstancia()
    {
        // Boundary: cualquier valor > 0 — 0.01 es el mínimo positivo razonable
        var act = () => new VariosDetalle("Marca", 0.01m, 5m, null, "Metal", false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Varios_Constructor_ConAltoCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 inválido — mata mutante <= → <
        var act = () => new VariosDetalle("Marca", 0m, 5m, null, "Metal", false);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalidas*");
    }

    [Fact]
    public void Varios_Constructor_ConAltoNegativo_DebeArrojarArgumentException()
    {
        var act = () => new VariosDetalle("Marca", -1m, 5m, null, "Metal", false);

        act.Should().Throw<ArgumentException>();
    }

    // ── Ancho > 0 (boundaries) — mata el mutante del segundo operando ─────────

    [Fact]
    public void Varios_Constructor_ConAnchoMinimo_DebeCrearInstancia()
    {
        var act = () => new VariosDetalle("Marca", 10m, 0.01m, null, "Metal", false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Varios_Constructor_ConAnchoCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 inválido — mata mutante del segundo operando del OR
        var act = () => new VariosDetalle("Marca", 10m, 0m, null, "Metal", false);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalidas*");
    }

    [Fact]
    public void Varios_Constructor_ConAnchoNegativo_DebeArrojarArgumentException()
    {
        var act = () => new VariosDetalle("Marca", 10m, -5m, null, "Metal", false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Varios_Constructor_AltoCeroYAnchoCero_DebeArrojar()
    {
        // Ambos inválidos a la vez
        var act = () => new VariosDetalle("Marca", 0m, 0m, null, "Metal", false);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void Varios_Actualizar_ConDatosValidos_DebeActualizarTodas()
    {
        var detalle = new VariosDetalle("Bandai", 15m, 10m, 5m, "Plástico", true);

        detalle.Actualizar("Funko", 20m, 12m, null, "Metal", false);

        detalle.Marca.Should().Be("Funko");
        detalle.Alto.Should().Be(20m);
        detalle.Ancho.Should().Be(12m);
        detalle.Largo.Should().BeNull();
        detalle.Material.Should().Be("Metal");
        detalle.TieneIlustracion.Should().BeFalse();
    }

    [Fact]
    public void Varios_Actualizar_ConDimensionInvalida_DebeArrojar()
    {
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        var act = () => detalle.Actualizar("Marca", 0m, 5m, null, "Metal", false);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: solo alto=0 testeado; ancho=0 sobrevive ──────────────────

    [Fact]
    public void Actualizar_AnchoCero_DebeArrojar()
    {
        // Mata el mutante que elimina "|| ancho <= 0" en Actualizar
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        var act = () => detalle.Actualizar("Marca", 10m, 0m, null, "Metal", false);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalidas*");
    }

    // ── Actualizar: Largo siempre null en tests — valor no-null sobrevive ─────

    [Fact]
    public void Actualizar_LargoConValor_SeActualizaCorrectamente()
    {
        // Mata el mutante Largo = largo → Largo = null en Actualizar
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        detalle.Actualizar("Marca", 10m, 5m, 20m, "Metal", false);

        detalle.Largo.Should().Be(20m);
    }

    // ── Constructor y Actualizar: TieneIlustracion false → true mutant ────────

    [Fact]
    public void Actualizar_TieneIlustracionFalse_SeActualizaCorrectamente()
    {
        // Detalle con TieneIlustracion=true, luego se actualiza a false
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", true);

        detalle.Actualizar("Marca", 10m, 5m, null, "Metal", false);

        // Mata el mutante !tieneIlustracion en la asignación
        detalle.TieneIlustracion.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_MaterialSeActualizaCorrectamente()
    {
        // Mata el mutante Material = material → Material = null en Actualizar
        var detalle = new VariosDetalle("Marca", 10m, 5m, null, "Metal", false);

        detalle.Actualizar("NuevaMarca", 10m, 5m, null, "Madera", false);

        detalle.Material.Should().Be("Madera");
    }
}
