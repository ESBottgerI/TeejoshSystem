using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Application.Ports.Outbound.Dashboard;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

public sealed class DashboardReadService : IDashboardReadService
{
    private readonly InventarioDbContext _db;
    public DashboardReadService(InventarioDbContext db) => _db = db;

    public async Task<DashboardReadModel> ReadAsync(DateTime fromInclusive, DateTime toExclusive, DateTime today, CancellationToken cancellationToken)
    {
        var tomorrow = today.AddDays(1);
        var products = await _db.Productos.AsNoTracking().CountAsync(cancellationToken);
        var noStock = await _db.Productos.AsNoTracking().CountAsync(p => p.Stock.Value <= 0, cancellationToken);
        var todaySales = await _db.Ventas.AsNoTracking().Where(v => v.Fecha >= today && v.Fecha < tomorrow).ToListAsync(cancellationToken);
        var sales = await _db.Ventas.AsNoTracking().Where(v => v.Fecha >= fromInclusive && v.Fecha < toExclusive).Select(v => new { v.Id, v.Fecha }).ToListAsync(cancellationToken);
        var saleIds = sales.Select(x => x.Id).ToArray();
        var details = await _db.VentaDetalles.AsNoTracking().Where(d => saleIds.Contains(d.VentaId))
            .Select(d => new { d.VentaId, d.ProductoId, d.NombreProducto, d.Cantidad, d.PrecioUnitario }).ToListAsync(cancellationToken);
        var saleTimes = sales.ToDictionary(x => x.Id, x => x.Fecha);
        var dates = sales.ToDictionary(x => x.Id, x => DateOnly.FromDateTime(x.Fecha));
        var productRows = details.GroupBy(d => d.ProductoId).Select(g => {
            var latest = g.OrderByDescending(d => saleTimes[d.VentaId]).ThenByDescending(d => d.VentaId).First();
            return new DashboardProductAggregate(g.Key, latest.NombreProducto, g.Sum(d => d.Cantidad * d.PrecioUnitario), g.Sum(d => d.Cantidad));
        }).ToList();
        var daily = details.GroupBy(d => dates[d.VentaId]).Select(g => new DashboardDailyAggregate(g.Key, g.Sum(d => d.Cantidad * d.PrecioUnitario))).ToList();
        return new DashboardReadModel(products, noStock, todaySales.Count, todaySales.Sum(v => v.Total), productRows, daily);
    }
}
