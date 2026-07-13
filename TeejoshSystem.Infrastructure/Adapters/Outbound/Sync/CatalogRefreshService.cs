using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Sync
{
    /// <summary>
    /// BackgroundService que refresca la réplica SQLite local con datos de Supabase
    /// cada vez que se recupera la conexión.
    ///
    /// Qué replica:
    ///   - Catálogos completos (HotWheels, Funko, TCG) — full replace
    ///   - Productos — upsert por Id (no elimina productos borrados remotamente en esta versión)
    ///   - Ventas recientes (últimos 30 días) — para que el historial offline sea útil
    ///
    /// Qué NO replica:
    ///   - Usuarios (seguridad — no deben existir passwords en local si no es necesario)
    ///   - sync_outbox (es exclusivamente local)
    /// </summary>
    public class CatalogRefreshService : BackgroundService
    {
        private readonly IConnectivityService _connectivity;
        private readonly IServiceScopeFactory _scopeFactory;
        private bool _refreshPending = false;

        public CatalogRefreshService(
            IConnectivityService connectivity,
            IServiceScopeFactory scopeFactory)
        {
            _connectivity = connectivity;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connectivity.ConnectivityChanged += async (isOnline) =>
            {
                if (isOnline && !stoppingToken.IsCancellationRequested)
                {
                    _refreshPending = true;
                    await RefreshAsync(stoppingToken);
                }
            };

            return Task.CompletedTask;
        }

        private async Task RefreshAsync(CancellationToken ct)
        {
            if (!_refreshPending) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var pgContext = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
                var localContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

                await RefreshCatalogosAsync(pgContext, localContext, ct);
                await RefreshProductosAsync(pgContext, localContext, ct);
                await RefreshVentasRecientesAsync(pgContext, localContext, ct);

                _refreshPending = false;
            }
            catch
            {
                // Fallo silencioso — el refresh se reintentará en la próxima reconexión
            }
        }

        private async Task RefreshCatalogosAsync(
            InventarioDbContext pg, LocalDbContext local, CancellationToken ct)
        {
            // HotWheels categorias
            var hwCats = await pg.HotWheelsCategorias.AsNoTracking().ToListAsync(ct);
            local.HotWheelsCategorias.RemoveRange(local.HotWheelsCategorias);
            await local.HotWheelsCategorias.AddRangeAsync(hwCats, ct);

            // Funko subtipos y características
            var funkoSubs = await pg.FunkoSubtipos.AsNoTracking().ToListAsync(ct);
            var funkoChars = await pg.FunkoCaracteristicas.AsNoTracking().ToListAsync(ct);
            local.FunkoSubtipos.RemoveRange(local.FunkoSubtipos);
            local.FunkoCaracteristicas.RemoveRange(local.FunkoCaracteristicas);
            await local.FunkoSubtipos.AddRangeAsync(funkoSubs, ct);
            await local.FunkoCaracteristicas.AddRangeAsync(funkoChars, ct);

            // TCG franquicias, expansiones, packs
            var franquicias = await pg.TcgFranquicias.AsNoTracking().ToListAsync(ct);
            var expansiones = await pg.TcgExpansiones.AsNoTracking().ToListAsync(ct);
            var packs = await pg.TcgPacks.AsNoTracking().ToListAsync(ct);
            local.TcgFranquicias.RemoveRange(local.TcgFranquicias);
            local.TcgExpansiones.RemoveRange(local.TcgExpansiones);
            local.TcgPacks.RemoveRange(local.TcgPacks);
            await local.TcgFranquicias.AddRangeAsync(franquicias, ct);
            await local.TcgExpansiones.AddRangeAsync(expansiones, ct);
            await local.TcgPacks.AddRangeAsync(packs, ct);

            await local.SaveChangesAsync(ct);
        }

        private async Task RefreshProductosAsync(
            InventarioDbContext pg, LocalDbContext local, CancellationToken ct)
        {
            var productos = await pg.Productos.AsNoTracking().ToListAsync(ct);

            foreach (var producto in productos)
            {
                var existing = await local.Productos.FindAsync(new object[] { producto.Id }, ct);
                if (existing is null)
                    await local.Productos.AddAsync(producto, ct);
                else
                    local.Entry(existing).CurrentValues.SetValues(producto);
            }

            await local.SaveChangesAsync(ct);
        }

        private async Task RefreshVentasRecientesAsync(
            InventarioDbContext pg, LocalDbContext local, CancellationToken ct)
        {
            var desde = DateTime.UtcNow.AddDays(-30);
            var ventas = await pg.Ventas
                .AsNoTracking()
                .Where(v => v.Fecha >= desde)
                .ToListAsync(ct);

            foreach (var venta in ventas)
            {
                var existing = await local.Ventas.FindAsync(new object[] { venta.Id }, ct);
                if (existing is null)
                    await local.Ventas.AddAsync(venta, ct);
            }

            await local.SaveChangesAsync(ct);
        }
    }
}