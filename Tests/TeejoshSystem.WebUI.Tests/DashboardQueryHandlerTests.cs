using FluentAssertions;
using NSubstitute;
using TeejoshSystem.Application.Ports.Inbound.Dashboard.ObtenerDashboard;
using TeejoshSystem.Application.Ports.Outbound.Dashboard;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class DashboardQueryHandlerTests
{
    [Fact]
    public async Task Handle_FillsMissingDaysWithZero()
    {
        var reader = Substitute.For<IDashboardReadService>();
        reader.ReadAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DashboardReadModel(2, 1, 0, 0, Array.Empty<DashboardProductAggregate>(), [new DashboardDailyAggregate(new DateOnly(2026, 7, 2), 20m)]));
        var handler = new ObtenerDashboardQueryHandler(reader, TimeProvider.System);
        var result = await handler.Handle(new ObtenerDashboardQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 3), CriterioRankingProducto.Ingresos), CancellationToken.None);
        result.IsSuccess.Should().BeTrue(); result.Value.IngresosDiarios.Select(x => x.Ingresos).Should().Equal(0m, 20m, 0m);
    }

    [Fact]
    public async Task Handle_InvertedRangeFailsWithoutReading()
    {
        var reader = Substitute.For<IDashboardReadService>();
        var handler = new ObtenerDashboardQueryHandler(reader, TimeProvider.System);
        var result = await handler.Handle(new ObtenerDashboardQuery(new DateOnly(2026, 7, 3), new DateOnly(2026, 7, 1), CriterioRankingProducto.Ingresos), CancellationToken.None);
        result.IsSuccess.Should().BeFalse(); await reader.DidNotReceiveWithAnyArgs().ReadAsync(default, default, default, default);
    }
}
