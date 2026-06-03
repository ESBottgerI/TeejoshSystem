using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities.Detalles;

public class HotWheelsDetalleTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // HotWheelsDetalle
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void HotWheels_Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        var detalle = new HotWheelsDetalle("Ferrari 250 GTO", 2020, "Treasure Hunt", 1);

        detalle.Modelo.Should().Be("Ferrari 250 GTO");
        detalle.Anio.Should().Be(2020);
        detalle.Serie.Should().Be("Treasure Hunt");
        detalle.CategoriaId.Should().Be(1);
    }

    // ── Anio — boundaries críticos ────────────────────────────────────────────

    [Fact]
    public void HotWheels_Constructor_ConAnio1967_DebeCrearInstancia()
    {
        // Boundary inferior válido: 1967 (primer año Hot Wheels)
        var act = () => new HotWheelsDetalle("Modelo", 1967, "Serie", 1);

        act.Should().NotThrow();
    }

    [Fact]
    public void HotWheels_Constructor_ConAnio1966_DebeArrojarArgumentException()
    {
        // Boundary inferior inválido: 1966 — mata mutante < → <=
        var act = () => new HotWheelsDetalle("Modelo", 1966, "Serie", 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalido*");
    }

    [Fact]
    public void HotWheels_Constructor_ConAnioActualMasUno_DebeCrearInstancia()
    {
        // Boundary superior válido: Year + 1 (modelos del próximo año permitidos)
        var anioValido = DateTime.Now.Year + 1;

        var act = () => new HotWheelsDetalle("Modelo", anioValido, "Serie", 1);

        act.Should().NotThrow();
    }

    [Fact]
    public void HotWheels_Constructor_ConAnioActualMasDos_DebeArrojarArgumentException()
    {
        // Boundary superior inválido: Year + 2 — mata mutante > → >=
        var anioInvalido = DateTime.Now.Year + 2;

        var act = () => new HotWheelsDetalle("Modelo", anioInvalido, "Serie", 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalido*");
    }

    // ── Modelo y Serie — IsNullOrWhiteSpace ───────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HotWheels_Constructor_ConModeloInvalido_DebeArrojarArgumentException(string? modelo)
    {
        var act = () => new HotWheelsDetalle(modelo!, 2020, "Serie", 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*modelo*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HotWheels_Constructor_ConSerieInvalida_DebeArrojarArgumentException(string? serie)
    {
        var act = () => new HotWheelsDetalle("Modelo", 2020, serie!, 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*serie*");
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    [Fact]
    public void HotWheels_Actualizar_ConDatosValidos_DebeActualizarPropiedades()
    {
        var detalle = new HotWheelsDetalle("OriginalModelo", 2020, "OriginalSerie", 1);

        detalle.Actualizar("NuevoModelo", 2023, "NuevaSerie", 2);

        detalle.Modelo.Should().Be("NuevoModelo");
        detalle.Anio.Should().Be(2023);
        detalle.Serie.Should().Be("NuevaSerie");
        detalle.CategoriaId.Should().Be(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HotWheels_Actualizar_ConModeloInvalido_DebeArrojar(string? modelo)
    {
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar(modelo!, 2020, "Serie", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HotWheels_Actualizar_ConAnioInvalido_DebeArrojar()
    {
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 1900, "Serie", 1);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: serie inválida (2 no cov + mutantes del guard) ────────────

    [Fact]
    public void Actualizar_SerieNull_DebeArrojar()
    {
        // Mata los mutantes del guard IsNullOrWhiteSpace(serie) en Actualizar
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 2020, null!, 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*serie*");
    }

    [Fact]
    public void Actualizar_SerieVacia_DebeArrojar()
    {
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 2020, "", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_SerieSoloEspacios_DebeArrojar()
    {
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 2020, "   ", 1);

        act.Should().Throw<ArgumentException>();
    }

    // ── Actualizar: boundaries de anio — ambos no testeados en Actualizar ─────

    [Fact]
    public void Actualizar_ConAnio1966_DebeArrojar()
    {
        // Boundary inferior inválido en Actualizar — mata mutante < → <=
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 1966, "Serie", 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalido*");
    }

    [Fact]
    public void Actualizar_ConAnio1967_NoDebeArrojar()
    {
        // Boundary inferior válido en Actualizar — par necesario para el mutante
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("Modelo", 1967, "Serie", 1);

        act.Should().NotThrow();
    }

    [Fact]
    public void Actualizar_ConAnioActualMasUno_NoDebeArrojar()
    {
        // Boundary superior válido en Actualizar
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);
        var anioValido = DateTime.Now.Year + 1;

        var act = () => detalle.Actualizar("Modelo", anioValido, "Serie", 1);

        act.Should().NotThrow();
    }

    [Fact]
    public void Actualizar_ConAnioActualMasDos_DebeArrojar()
    {
        // Boundary superior inválido en Actualizar — mata mutante > → >=
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);
        var anioInvalido = DateTime.Now.Year + 2;

        var act = () => detalle.Actualizar("Modelo", anioInvalido, "Serie", 1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*invalido*");
    }

    // ── Actualizar: modelo whitespace (solo null y "" ya cubiertos) ───────────

    [Fact]
    public void Actualizar_ModeloSoloEspacios_DebeArrojar()
    {
        // Mata el mutante residual de IsNullOrWhiteSpace(modelo) en Actualizar
        var detalle = new HotWheelsDetalle("Modelo", 2020, "Serie", 1);

        var act = () => detalle.Actualizar("   ", 2020, "Serie", 1);

        act.Should().Throw<ArgumentException>();
    }
}
