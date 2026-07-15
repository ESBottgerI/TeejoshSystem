namespace TeejoshSystem.Application.Ports.Outbound.Dashboard;

public interface IDashboardReadService
{
    Task<DashboardReadModel> ReadAsync(DateTime fromInclusive, DateTime toExclusive, DateTime today, CancellationToken cancellationToken);
}

public sealed record DashboardReadModel(
    int Productos,
    int ProductosSinStock,
    int VentasHoy,
    decimal IngresosHoy,
    IReadOnlyList<DashboardProductAggregate> ProductosVendidos,
    IReadOnlyList<DashboardDailyAggregate> IngresosDiarios);

public sealed record DashboardProductAggregate(int ProductoId, string Nombre, decimal Ingresos, int UnidadesVendidas);
public sealed record DashboardDailyAggregate(DateOnly Fecha, decimal Ingresos);
