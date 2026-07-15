using FluentAssertions;
using NSubstitute;
using TeejoshSystem.Application.Ports.Inbound.Dashboard.ObtenerDashboard;
using TeejoshSystem.Application.Ports.Outbound.Dashboard;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class DashboardRankingTests
{
    [Theory]
    [InlineData(CriterioRankingProducto.Ingresos, 2)]
    [InlineData(CriterioRankingProducto.UnidadesVendidas, 1)]
    public async Task Handle_OrdersByRequestedCriterion(CriterioRankingProducto criterio, int expectedFirstId)
    {
        var products = new[]
        {
            new DashboardProductAggregate(1, "Muchas unidades", 80m, 8),
            new DashboardProductAggregate(2, "Mayores ingresos", 100m, 2)
        };
        var result = await HandleAsync(products, criterio);

        result.IsSuccess.Should().BeTrue();
        result.Value.ProductosTop.First().ProductoId.Should().Be(expectedFirstId);
    }

    [Fact]
    public async Task Handle_UsesNameAndIdAsDeterministicTieBreakers()
    {
        var products = new[]
        {
            new DashboardProductAggregate(3, "beta", 50m, 5),
            new DashboardProductAggregate(2, "Alpha", 50m, 5),
            new DashboardProductAggregate(1, "alpha", 50m, 5)
        };
        var result = await HandleAsync(products, CriterioRankingProducto.Ingresos);

        result.Value.ProductosTop.Select(x => x.ProductoId).Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Handle_ReturnsAtMostTenProducts()
    {
        var products = Enumerable.Range(1, 12)
            .Select(id => new DashboardProductAggregate(id, $"Producto {id}", id, id))
            .ToArray();
        var result = await HandleAsync(products, CriterioRankingProducto.Ingresos);

        result.Value.ProductosTop.Should().HaveCount(10);
        result.Value.ProductosTop.Select(x => x.ProductoId).Should().Equal(12, 11, 10, 9, 8, 7, 6, 5, 4, 3);
    }

    [Fact]
    public async Task Handle_WhenReaderThrows_ReturnsControlledFailure()
    {
        var reader = Substitute.For<IDashboardReadService>();
        reader.ReadAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<DashboardReadModel>>(_ => throw new InvalidOperationException("database unavailable"));
        var handler = new ObtenerDashboardQueryHandler(reader, TimeProvider.System);

        var result = await handler.Handle(
            new ObtenerDashboardQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 1), CriterioRankingProducto.Ingresos),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No se pudo cargar el Dashboard.");
    }

    private static async Task<TeejoshSystem.Application.Common.Result<DashboardDto>> HandleAsync(
        IReadOnlyList<DashboardProductAggregate> products,
        CriterioRankingProducto criterion)
    {
        var reader = Substitute.For<IDashboardReadService>();
        reader.ReadAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DashboardReadModel(12, 2, 3, 150m, products, Array.Empty<DashboardDailyAggregate>()));
        var handler = new ObtenerDashboardQueryHandler(reader, TimeProvider.System);
        return await handler.Handle(
            new ObtenerDashboardQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 1), criterion),
            CancellationToken.None);
    }
}
