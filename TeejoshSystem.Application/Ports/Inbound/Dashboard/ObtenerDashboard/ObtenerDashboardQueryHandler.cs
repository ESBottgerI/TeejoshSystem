using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Outbound.Dashboard;

namespace TeejoshSystem.Application.Ports.Inbound.Dashboard.ObtenerDashboard;

public sealed class ObtenerDashboardQueryHandler : IRequestHandler<ObtenerDashboardQuery, Result<DashboardDto>>
{
    private readonly IDashboardReadService _reader;
    private readonly TimeProvider _clock;
    public ObtenerDashboardQueryHandler(IDashboardReadService reader, TimeProvider clock) { _reader = reader; _clock = clock; }

    public async Task<Result<DashboardDto>> Handle(ObtenerDashboardQuery request, CancellationToken cancellationToken)
    {
        if (request.Desde > request.Hasta) return Result.Failure<DashboardDto>("El rango de fechas está invertido.");
        try
        {
            var from = request.Desde.ToDateTime(TimeOnly.MinValue);
            var to = request.Hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);
            var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime).ToDateTime(TimeOnly.MinValue);
            var raw = await _reader.ReadAsync(from, to, today, cancellationToken);
            var ordered = request.Criterio == CriterioRankingProducto.Ingresos
                ? raw.ProductosVendidos.OrderByDescending(x => x.Ingresos).ThenByDescending(x => x.UnidadesVendidas)
                : raw.ProductosVendidos.OrderByDescending(x => x.UnidadesVendidas).ThenByDescending(x => x.Ingresos);
            var top = ordered.ThenBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.ProductoId).Take(10)
                .Select(x => new ProductoTopDto(x.ProductoId, x.Nombre, x.Ingresos, x.UnidadesVendidas)).ToList();
            var byDate = raw.IngresosDiarios.ToDictionary(x => x.Fecha, x => x.Ingresos);
            var daily = new List<IngresoDiarioDto>();
            for (var day = request.Desde; day <= request.Hasta; day = day.AddDays(1))
                daily.Add(new IngresoDiarioDto(day, byDate.GetValueOrDefault(day)));
            return Result.Success(new DashboardDto(raw.Productos, raw.ProductosSinStock, raw.VentasHoy, raw.IngresosHoy, top, daily));
        }
        catch { return Result.Failure<DashboardDto>("No se pudo cargar el Dashboard."); }
    }
}
