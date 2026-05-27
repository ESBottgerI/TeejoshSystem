using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Tests.ValueObjects;

/// <summary>
/// Tests de NombreProducto.
/// Diseñados para matar mutantes de Stryker en:
///   - condición IsNullOrWhiteSpace  (null, empty, whitespace)
///   - condición length > 100        (boundary 100 y 101)
///   - Trim() aplicado               (espacios removidos)
///   - Equals / GetHashCode          (value object equality)
///   - ToString                      (retorna Value)
/// </summary>
public class NombreProductoTests
{
    // ── Construcción válida ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ConNombreValido_DebeCrearInstancia()
    {
        var nombre = new NombreProducto("Pikachu V");

        nombre.Value.Should().Be("Pikachu V");
    }

    [Fact]
    public void Constructor_ConNombreDeUnCaracter_DebeCrearInstancia()
    {
        // Límite inferior: 1 carácter es válido
        var nombre = new NombreProducto("X");

        nombre.Value.Should().Be("X");
    }

    [Fact]
    public void Constructor_ConNombreDeExactamente100Caracteres_DebeCrearInstancia()
    {
        // Boundary: 100 caracteres es el límite permitido
        var valor = new string('A', 100);

        var act = () => new NombreProducto(valor);

        act.Should().NotThrow();
    }

    // ── Trim ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ConEspaciosAlRededor_DebeAplicarTrim()
    {
        var nombre = new NombreProducto("  Charizard EX  ");

        // Trim debe haber sido aplicado; no se almacena con espacios
        nombre.Value.Should().Be("Charizard EX");
    }

    [Fact]
    public void Constructor_ConEspacioInternoSinBordes_NoAlteraNombre()
    {
        // Trim solo elimina extremos, no espacios internos
        var nombre = new NombreProducto("Hot Wheels Edicion Especial");

        nombre.Value.Should().Be("Hot Wheels Edicion Especial");
    }

    // ── Construcción inválida — mutantes IsNullOrWhiteSpace ───────────────────

    [Fact]
    public void Constructor_ConNull_DebeArrojarArgumentException()
    {
        var act = () => new NombreProducto(null!);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*obligatorio*");
    }

    [Fact]
    public void Constructor_ConStringVacia_DebeArrojarArgumentException()
    {
        var act = () => new NombreProducto(string.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*obligatorio*");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_ConSoloEspaciosOWhitespace_DebeArrojarArgumentException(string espacio)
    {
        var act = () => new NombreProducto(espacio);

        act.Should().Throw<ArgumentException>();
    }

    // ── Construcción inválida — boundary length > 100 ────────────────────────

    [Fact]
    public void Constructor_ConNombreDeExactamente101Caracteres_DebeArrojarArgumentException()
    {
        // Boundary superior: 101 debe fallar
        var valor = new string('A', 101);

        var act = () => new NombreProducto(valor);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*100*");
    }

    [Theory]
    [InlineData(102)]
    [InlineData(200)]
    public void Constructor_ConNombreMayorA100Caracteres_DebeArrojarArgumentException(int longitud)
    {
        var valor = new string('B', longitud);

        var act = () => new NombreProducto(valor);

        act.Should().Throw<ArgumentException>();
    }

    // ── Igualdad — value object semantics ────────────────────────────────────

    [Fact]
    public void Equals_DosInstanciasConMismoValor_DebenSerIguales()
    {
        var a = new NombreProducto("Pikachu V");
        var b = new NombreProducto("Pikachu V");

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DosInstanciasConMismoValorConEspacios_DebenSerIguales()
    {
        // Trim hace que "  Pikachu V  " y "Pikachu V" sean equivalentes
        var a = new NombreProducto("  Pikachu V  ");
        var b = new NombreProducto("Pikachu V");

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_InstanciasConDistintoValor_NoDebenSerIguales()
    {
        var a = new NombreProducto("Pikachu V");
        var b = new NombreProducto("Charizard EX");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_ConNull_DebeRetornarFalse()
    {
        var nombre = new NombreProducto("Pikachu V");

        nombre.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ConObjetoDeOtroTipo_DebeRetornarFalse()
    {
        var nombre = new NombreProducto("Pikachu V");

        nombre.Equals("Pikachu V").Should().BeFalse();
    }

    // ── GetHashCode ───────────────────────────────────────────────────────────

    [Fact]
    public void GetHashCode_DosInstanciasIguales_DebenTenerMismoHash()
    {
        var a = new NombreProducto("Charizard");
        var b = new NombreProducto("Charizard");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_InstanciasDistintas_DebenTenerHashesDiferentes()
    {
        // No es garantía absoluta (colisiones), pero para valores diferentes es esperable
        var a = new NombreProducto("Pikachu");
        var b = new NombreProducto("Charizard");

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_DebeRetornarElValueDirectamente()
    {
        var nombre = new NombreProducto("Funko Batman");

        nombre.ToString().Should().Be("Funko Batman");
    }

    [Fact]
    public void ToString_CoincidesConValue()
    {
        var nombre = new NombreProducto("Hot Wheels Ferrari");

        nombre.ToString().Should().Be(nombre.Value);
    }
}