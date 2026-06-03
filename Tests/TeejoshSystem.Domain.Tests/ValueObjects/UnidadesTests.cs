using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Tests.ValueObjects;

public class UnidadesTests
{
    // ── Construcción válida ───────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    public void Constructor_ConValorNoNegativo_DebeCrearInstancia(int valor)
    {
        var unidades = new Unidades(valor);

        unidades.Value.Should().Be(valor);
    }

    // ── Construcción inválida ─────────────────────────────────────────────────

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ConValorNegativo_DebeArrojarArgumentException(int valor)
    {
        var act = () => new Unidades(valor);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*negativas*");
    }

    // ── Decrementar ───────────────────────────────────────────────────────────

    [Fact]
    public void Decrementar_ConStockSuficiente_DebeRetornarNuevaInstanciaReducida()
    {
        var original = new Unidades(10);

        var resultado = original.Decrementar(3);

        // inmutabilidad: original no cambia
        original.Value.Should().Be(10);
        resultado.Value.Should().Be(7);
    }

    [Fact]
    public void Decrementar_ExactamenteElStock_DebeDejarEnCero()
    {
        var unidades = new Unidades(5);

        var resultado = unidades.Decrementar(5);

        resultado.Value.Should().Be(0);
    }

    [Fact]
    public void Decrementar_CantidadMayorAlStock_DebeArrojarInvalidOperationException()
    {
        var unidades = new Unidades(2);

        var act = () => unidades.Decrementar(5);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public void Decrementar_StockEnCeroConCualquierCantidad_DebeArrojarInvalidOperationException()
    {
        var unidades = new Unidades(0);

        var act = () => unidades.Decrementar(1);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── Incrementar ───────────────────────────────────────────────────────────

    [Fact]
    public void Incrementar_DebeRetornarNuevaInstanciaIncrementada()
    {
        var original = new Unidades(5);

        var resultado = original.Incrementar(3);

        original.Value.Should().Be(5); // inmutabilidad
        resultado.Value.Should().Be(8);
    }

    [Fact]
    public void Incrementar_DesdeStockCero_DebeRetornarCantidadAgregada()
    {
        var unidades = new Unidades(0);

        var resultado = unidades.Incrementar(10);

        resultado.Value.Should().Be(10);
    }

    // ── Igualdad ──────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_DosInstanciasConMismoValor_DebenSerIguales()
    {
        var a = new Unidades(7);
        var b = new Unidades(7);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_InstanciasConDistintoValor_NoDebenSerIguales()
    {
        var a = new Unidades(3);
        var b = new Unidades(4);

        a.Should().NotBe(b);
    }
}
