using System.Text.Json;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Routing
{
    /// <summary>
    /// Repositorio de enrutamiento para Producto.
    /// 
    /// LECTURAS: siempre desde el contexto activo (PostgreSQL online / SQLite offline).
    /// ESCRITURAS online: PostgreSQL directo via SupabaseRepository.
    /// ESCRITURAS offline: SQLite local + encola en outbox para sincronizar al reconectar.
    ///
    /// Los handlers de Application no cambian — ven solo IProductoRepository.
    /// </summary>
    public class RoutingProductoRepository : IProductoRepository
    {
        private readonly IConnectivityService _connectivity;
        private readonly ISyncOutboxRepository _outbox;
        private readonly InventarioDbContext _pgContext;       // PostgreSQL
        private readonly LocalDbContext _localContext;         // SQLite
        private readonly string _deviceId;

        // Repositorios concretos instanciados bajo demanda
        private ProductoRepository PgRepo => new(_pgContext);
        private ProductoRepository LocalRepo => new(_localContext);

        public RoutingProductoRepository(
            IConnectivityService connectivity,
            ISyncOutboxRepository outbox,
            InventarioDbContext pgContext,
            LocalDbContext localContext,
            string deviceId)
        {
            _connectivity = connectivity;
            _outbox = outbox;
            _pgContext = pgContext;
            _localContext = localContext;
            _deviceId = deviceId;
        }

        // ── LECTURAS — siempre desde el contexto activo ──────────────────────

        public Task<IReadOnlyList<Producto>> GetAllAsync()
            => ActiveRepo().GetAllAsync();

        public Task<Producto?> GetByIdAsync(int id)
            => ActiveRepo().GetByIdAsync(id);

        public Task<IReadOnlyList<Producto>> SearchAsync(string? nombre, TipoProducto? tipo)
            => ActiveRepo().SearchAsync(nombre, tipo);

        public Task<Producto?> GetByIdWithDetalleAsync(int id)
            => ActiveRepo().GetByIdWithDetalleAsync(id);

        public Task<IReadOnlyList<ProductoBusquedaResult>> SearchWithDetalleAsync(
            string? nombre, TipoProducto? tipo)
            => ActiveRepo().SearchWithDetalleAsync(nombre, tipo);

        public Task<bool> ExistsAsync(int id)
            => ActiveRepo().ExistsAsync(id);

        // ── ESCRITURAS — con routing y outbox ────────────────────────────────

        public async Task<int> AddAsync(Producto producto)
        {
            if (_connectivity.IsOnline)
            {
                var id = await PgRepo.AddAsync(producto);
                // Replicar en local para mantener réplica actualizada
                await ReplicateInsertLocalAsync(producto);
                return id;
            }

            // Offline: escribir local + encolar
            var localId = await LocalRepo.AddAsync(producto);
            await EnqueueAsync("INSERT", "product", localId, producto);
            return localId;
        }

        public async Task UpdateAsync(Producto producto)
        {
            if (_connectivity.IsOnline)
            {
                await PgRepo.UpdateAsync(producto);
                await ReplicateUpdateLocalAsync(producto);
                return;
            }

            await LocalRepo.UpdateAsync(producto);
            await EnqueueAsync("UPDATE", "product", producto.Id, producto);
        }

        public async Task DeleteAsync(Producto producto)
        {
            if (_connectivity.IsOnline)
            {
                await PgRepo.DeleteAsync(producto);
                await DeleteLocalIfExistsAsync(producto.Id);
                return;
            }

            await LocalRepo.DeleteAsync(producto);
            await EnqueueAsync("DELETE", "product", producto.Id, null);
        }

        public async Task DeleteRangeAsync(IEnumerable<int> productoIds)
        {
            var ids = productoIds.ToList();
            if (_connectivity.IsOnline)
            {
                await PgRepo.DeleteRangeAsync(ids);
                foreach (var id in ids)
                    await DeleteLocalIfExistsAsync(id);
                return;
            }

            await LocalRepo.DeleteRangeAsync(ids);
            foreach (var id in ids)
                await EnqueueAsync("DELETE", "product", id, null);
        }

        // ── Detalles: Crear ──────────────────────────────────────────────────

        public async Task AddHotWheelsDetalleAsync(HotWheelsDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.AddHotWheelsDetalleAsync(detalle); return; }
            await LocalRepo.AddHotWheelsDetalleAsync(detalle);
            await EnqueueAsync("INSERT", "hot_wheels", detalle.ProductoId, detalle);
        }

        public async Task AddFunkoDetalleAsync(FunkoDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.AddFunkoDetalleAsync(detalle); return; }
            await LocalRepo.AddFunkoDetalleAsync(detalle);
            await EnqueueAsync("INSERT", "funko", detalle.ProductoId, detalle);
        }

        public async Task AddTcgDetalleAsync(TcgDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.AddTcgDetalleAsync(detalle); return; }
            await LocalRepo.AddTcgDetalleAsync(detalle);
            await EnqueueAsync("INSERT", "tcg", detalle.ProductoId, detalle);
        }

        public async Task AddToyDetalleAsync(ToyDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.AddToyDetalleAsync(detalle); return; }
            await LocalRepo.AddToyDetalleAsync(detalle);
            await EnqueueAsync("INSERT", "toy", detalle.ProductoId, detalle);
        }

        public async Task AddVariosDetalleAsync(VariosDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.AddVariosDetalleAsync(detalle); return; }
            await LocalRepo.AddVariosDetalleAsync(detalle);
            await EnqueueAsync("INSERT", "varios", detalle.ProductoId, detalle);
        }

        // ── Detalles: Actualizar ─────────────────────────────────────────────

        public async Task UpdateHotWheelsDetalleAsync(HotWheelsDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.UpdateHotWheelsDetalleAsync(detalle); return; }
            await LocalRepo.UpdateHotWheelsDetalleAsync(detalle);
            await EnqueueAsync("UPDATE", "hot_wheels", detalle.ProductoId, detalle);
        }

        public async Task UpdateFunkoDetalleAsync(FunkoDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.UpdateFunkoDetalleAsync(detalle); return; }
            await LocalRepo.UpdateFunkoDetalleAsync(detalle);
            await EnqueueAsync("UPDATE", "funko", detalle.ProductoId, detalle);
        }

        public async Task UpdateTcgDetalleAsync(TcgDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.UpdateTcgDetalleAsync(detalle); return; }
            await LocalRepo.UpdateTcgDetalleAsync(detalle);
            await EnqueueAsync("UPDATE", "tcg", detalle.ProductoId, detalle);
        }

        public async Task UpdateToyDetalleAsync(ToyDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.UpdateToyDetalleAsync(detalle); return; }
            await LocalRepo.UpdateToyDetalleAsync(detalle);
            await EnqueueAsync("UPDATE", "toy", detalle.ProductoId, detalle);
        }

        public async Task UpdateVariosDetalleAsync(VariosDetalle detalle)
        {
            if (_connectivity.IsOnline) { await PgRepo.UpdateVariosDetalleAsync(detalle); return; }
            await LocalRepo.UpdateVariosDetalleAsync(detalle);
            await EnqueueAsync("UPDATE", "varios", detalle.ProductoId, detalle);
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private ProductoRepository ActiveRepo()
            => _connectivity.IsOnline ? PgRepo : LocalRepo;

        private async Task EnqueueAsync(
            string operation, string table, int? entityId, object? payload)
        {
            await _outbox.EnqueueAsync(new SyncOutboxEntry
            {
                OperationType = operation,
                EntityTable   = table,
                EntityId      = entityId,
                PayloadJson   = payload is null
                    ? "{}"
                    : JsonSerializer.Serialize(payload),
                DeviceId      = _deviceId
            });
        }

        private async Task ReplicateInsertLocalAsync(Producto producto)
        {
            try { await LocalRepo.AddAsync(producto); } catch { /* réplica best-effort */ }
        }

        private async Task ReplicateUpdateLocalAsync(Producto producto)
        {
            try
            {
                if (await LocalRepo.ExistsAsync(producto.Id))
                    await LocalRepo.UpdateAsync(producto);
            }
            catch { /* best-effort */ }
        }

        private async Task DeleteLocalIfExistsAsync(int id)
        {
            try
            {
                var local = await LocalRepo.GetByIdAsync(id);
                if (local is not null) await LocalRepo.DeleteAsync(local);
            }
            catch { /* best-effort */ }
        }
    }
}