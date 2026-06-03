using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class ToyDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // ToyDetalle
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Toy_Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        var detalle = new ToyDetalle(3, 2, 4, false);

        detalle.EdadMinima.Should().Be(3);
        detalle.JugadoresMin.Should().Be(2);
        detalle.JugadoresMax.Should().Be(4);
        detalle.EsJuegoDeMesa.Should().BeFalse();
    }

    [Fact]
    public void Toy_Constructor_EsJuegoDeMesaTrue_DebeAsignar()
    {
        var detalle = new ToyDetalle(8, 2, 6, true);

        detalle.EsJuegoDeMesa.Should().BeTrue();
    }

    // ── JugadoresMin == JugadoresMax (boundary) ───────────────────────────────

    [Fact]
    public void Toy_Constructor_ConJugadoresMinIgualAMax_DebeCrearInstancia()
    {
        // Boundary: igual es válido — mata mutante < → <=
        var act = () => new ToyDetalle(3, 2, 2, false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Toy_Constructor_ConJugadoresMaxMenorQueMin_DebeArrojarArgumentException()
    {
        // Boundary: max < min inválido
        var act = () => new ToyDetalle(3, 4, 2, false);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*jugadores*");
    }

    [Theory]
    [InlineData(5, 1)]   // max=1 < min=5
    [InlineData(10, 9)]  // max=9 < min=10
    public void Toy_Constructor_ConRangoInvertido_DebeArrojar(int jugMin, int jugMax)
    {
        var act = () => new ToyDetalle(3, jugMin, jugMax, false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Toy_Constructor_ConJugadoresMaxMayorQueMin_EsValido()
    {
        var act = () => new ToyDetalle(5, 1, 6, true);

        act.Should().NotThrow();
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void Toy_Actualizar_ConDatosValidos_DebeActualizarPropiedades()
    {
        var detalle = new ToyDetalle(3, 2, 4, false);

        detalle.Actualizar(6, 1, 8, true);

        detalle.EdadMinima.Should().Be(6);
        detalle.JugadoresMin.Should().Be(1);
        detalle.JugadoresMax.Should().Be(8);
        detalle.EsJuegoDeMesa.Should().BeTrue();
    }

    [Fact]
    public void Toy_Actualizar_ConRangoInvalido_DebeArrojar()
    {
        var detalle = new ToyDetalle(3, 2, 4, false);

        var act = () => detalle.Actualizar(3, 5, 2, false);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: boundary jugadoresMin == jugadoresMax (solo en constructor) ─

    [Fact]
    public void Actualizar_JugadoresMinIgualAMax_NoDebeArrojar()
    {
        // El test de boundary igual solo existe para el constructor.
        // Este mata el mutante < → <= en Actualizar
        var detalle = new ToyDetalle(3, 2, 4, false);

        var act = () => detalle.Actualizar(3, 2, 2, false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Actualizar_JugadoresMaxMenorQueMin_DebeArrojar()
    {
        // Par obligatorio del test anterior
        var detalle = new ToyDetalle(3, 2, 4, false);

        var act = () => detalle.Actualizar(3, 4, 2, false);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*jugadores*");
    }

    [Fact]
    public void Actualizar_EdadMinima_SeActualizaCorrectamente()
    {
        // Mata el mutante que reemplaza EdadMinima = edadMinima en Actualizar
        var detalle = new ToyDetalle(3, 2, 4, false);

        detalle.Actualizar(12, 1, 6, false);

        detalle.EdadMinima.Should().Be(12);
    }
}
