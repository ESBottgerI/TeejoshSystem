using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Tests.ValueObjects;

public class PrecioTests
{
    // ── Construcción válida ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ConValorPositivo_DebeCrearInstancia()
    {
        var precio = new Precio(99.99m);

        precio.Value.Should().Be(99.99m);
    }

    [Fact]
    public void Constructor_ConCero_DebeCrearInstancia()
    {
        // Cero es válido — solo negativo está prohibido
        var precio = new Precio(0m);

        precio.Value.Should().Be(0m);
    }

    // ── Redondeo ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(10.555,  10.56)] // redondea al alza
    [InlineData(10.554,  10.55)] // redondea a la baja
    [InlineData(10.0,    10.00)] // sin decimales queda igual
    [InlineData(0.001,    0.00)] // sub-centavo → cero
    public void Constructor_DebeRedondearADosDecimales(decimal entrada, decimal esperado)
    {
        var precio = new Precio(entrada);

        precio.Value.Should().Be(esperado);
    }

    // ── Construcción inválida ─────────────────────────────────────────────────

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-999.99)]
    public void Constructor_ConValorNegativo_DebeArrojarArgumentException(decimal valor)
    {
        var act = () => new Precio(valor);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*negativo*");
    }

    // ── Igualdad ──────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_DosInstanciasConMismoValor_DebenSerIguales()
    {
        var a = new Precio(25.00m);
        var b = new Precio(25.00m);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_InstanciasConDistintoValor_NoDebenSerIguales()
    {
        var a = new Precio(10.00m);
        var b = new Precio(20.00m);

        a.Should().NotBe(b);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(5,    "5.00")]
    [InlineData(0,    "0.00")]
    [InlineData(9.99, "9.99")]
    public void ToString_DebeFormatearConDosDecimales(decimal valor, string esperado)
    {
        var precio = new Precio(valor);

        precio.ToString().Should().Be(esperado);
    }

    // ── decimal.Round(value, 2): nunca verificado con decimales extras ────────

    [Fact]
    public void Constructor_ConMasDeDosDecimales_AplicaRedondeo()
    {
        // Mata el mutante: decimal.Round(value, 2) → value
        // 10.567 redondeado a 2 decimales = 10.57
        var precio = new Precio(10.567m);

        precio.Value.Should().Be(10.57m);
    }

    [Fact]
    public void Constructor_ConTresDecimales_RedondeaAlSegundo()
    {
        // Segundo caso de redondeo — refuerza que Round se aplica
        var precio = new Precio(5.005m);

        precio.Value.Should().Be(5.01m);
    }

    [Fact]
    public void Constructor_ConDosDecimalesExactos_NoCambiaElValor()
    {
        // Verifica que Round no altera valores ya correctos
        var precio = new Precio(25.99m);

        precio.Value.Should().Be(25.99m);
    }

    // ── ToString("0.00"): nunca testeado el formato ───────────────────────────

    [Fact]
    public void ToString_ConEntero_MuestraDosDecimales()
    {
        // Mata el mutante: "0.00" → diferente formato
        var precio = new Precio(10m);

        precio.ToString().Should().Be("10.00");
    }

    [Fact]
    public void ToString_ConDecimal_MuestraFormatoCorrecto()
    {
        var precio = new Precio(5.50m);

        precio.ToString().Should().Be("5.50");
    }

    // ── Equals: mutante Value == other.Value → true/false constante ──────────

    [Fact]
    public void Equals_ConTipoDistinto_RetornaFalse()
    {
        // Mata el mutante: obj is Precio other → true (siempre hace cast)
        var precio = new Precio(10m);

        precio.Equals("10.00").Should().BeFalse();
        precio.Equals(10m).Should().BeFalse();
        precio.Equals(null).Should().BeFalse();
    }
}
