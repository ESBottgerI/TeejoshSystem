using MediatR;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Dashboard.ObtenerDashboard;

public enum CriterioRankingProducto { Ingresos, UnidadesVendidas }

public sealed record ObtenerDashboardQuery(DateOnly Desde, DateOnly Hasta, CriterioRankingProducto Criterio)
    : IRequest<Result<DashboardDto>>;

public sealed record DashboardDto(int Productos, int ProductosSinStock, int VentasHoy, decimal IngresosHoy,
    IReadOnlyList<ProductoTopDto> ProductosTop, IReadOnlyList<IngresoDiarioDto> IngresosDiarios);
public sealed record ProductoTopDto(int ProductoId, string NombreHistorico, decimal Ingresos, int UnidadesVendidas);
public sealed record IngresoDiarioDto(DateOnly Fecha, decimal Ingresos);
