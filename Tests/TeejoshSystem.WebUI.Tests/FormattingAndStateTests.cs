using FluentAssertions;
using TeejoshSystem.Application.Common.Formatting;
using TeejoshSystem.WebUI.Infrastructure.State;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class FormattingAndStateTests
{
    [Theory]
    [InlineData(0, "S/ 0.00")]
    [InlineData(12.5, "S/ 12.50")]
    [InlineData(1234.56, "S/ 1,234.56")]
    public void SolesFormatter_IsDeterministic(decimal value, string expected) => SolesFormatter.Format(value).Should().Be(expected);

    [Fact]
    public void CircuitStores_AreIsolated()
    {
        var first = new CircuitStateStore(); var second = new CircuitStateStore();
        first.AddOrIncrement(1, "Producto", 10m, 2);
        first.CartItems.Should().ContainSingle(); second.CartItems.Should().BeEmpty();
    }

    [Fact]
    public void Clear_RemovesCartItems()
    {
        var state = new CircuitStateStore(); state.AddOrIncrement(1, "Producto", 10m, 2);
        state.Clear(); state.CartItems.Should().BeEmpty();
    }
}
