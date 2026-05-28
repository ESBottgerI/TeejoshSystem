using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Domain.Tests.Entities;

/// <summary>
/// Tests de los 5 detalles de producto + ProductoDetalle.AsignarProductoId.
///
/// Mutantes objetivo por clase:
///
/// ProductoDetalle.AsignarProductoId:
///   - productoId &lt;= 0 → boundaries 0 (inválido) y 1 (válido)
///
/// HotWheelsDetalle:
///   - IsNullOrWhiteSpace(modelo) y IsNullOrWhiteSpace(serie)
///   - anio &lt; 1967  → boundary 1966 (inválido) y 1967 (válido)
///   - anio > Year+1 → boundary Year+1 (válido) y Year+2 (inválido)
///
/// FunkoDetalle:
///   - numeroCaja &lt;= 0 → boundaries 0 y 1
///   - IsNullOrWhiteSpace(licencia)
///   - CaracteristicaEspecialId nullable (null permitido)
///
/// TcgDetalle:
///   - Sin guards aritméticos → tests de asignación y Actualizar
///
/// ToyDetalle:
///   - jugadoresMax &lt; jugadoresMin → boundary igual (válido) y invertido (inválido)
///
/// VariosDetalle:
///   - alto &lt;= 0 y ancho &lt;= 0 → boundaries 0 y 0.01
///   - largo nullable → null permitido, valor positivo permitido
/// </summary>
public class DetallesTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // ProductoDetalle.AsignarProductoId
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void AsignarProductoId_ConIdUno_DebeAsignarCorrectamente()
    {
        // Boundary inferior válido: 1
        var detalle = new TcgDetalle(1, 1);

        detalle.AsignarProductoId(1);

        detalle.ProductoId.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    public void AsignarProductoId_ConIdPositivo_DebeAsignar(int id)
    {
        var detalle = new TcgDetalle(1, 1);

        detalle.AsignarProductoId(id);

        detalle.ProductoId.Should().Be(id);
    }

    [Fact]
    public void AsignarProductoId_ConCero_DebeArrojarArgumentException()
    {
        // Boundary: 0 inválido — mata el mutante <= → <
        var detalle = new TcgDetalle(1, 1);

        var act = () => detalle.AsignarProductoId(0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*mayor a 0*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AsignarProductoId_ConNegativo_DebeArrojarArgumentException(int id)
    {
        var detalle = new TcgDetalle(1, 1);

        var act = () => detalle.AsignarProductoId(id);

        act.Should().Throw<ArgumentException>();
    }

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

    // ═════════════════════════════════════════════════════════════════════════
    // TcgDetalle — sin guards aritméticos, tests de asignación y Actualizar
    // ═════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Tcg_Constructor_DebeAsignarPackIdYExpansionId()
    {
        var detalle = new TcgDetalle(10, 25);

        detalle.PackId.Should().Be(10);
        detalle.ExpansionId.Should().Be(25);
    }

    [Fact]
    public void Tcg_Constructor_ConValoresDistintos_DebeAsignarCorrectamente()
    {
        var detalle = new TcgDetalle(99, 1);

        detalle.PackId.Should().Be(99);
        detalle.ExpansionId.Should().Be(1);
    }

    [Fact]
    public void Tcg_Actualizar_DebeModificarPackIdYExpansionId()
    {
        var detalle = new TcgDetalle(10, 25);

        detalle.Actualizar(50, 100);

        detalle.PackId.Should().Be(50);
        detalle.ExpansionId.Should().Be(100);
    }

    [Fact]
    public void Tcg_Actualizar_DebePoderAsignarMismoValor()
    {
        // Actualizar con los mismos valores no debe lanzar
        var detalle = new TcgDetalle(10, 25);

        var act = () => detalle.Actualizar(10, 25);

        act.Should().NotThrow();
    }

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