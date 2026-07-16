using System.Text.Json;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Routing
{
    /// <summary>
    /// Repositorio de enrutamiento para Venta.
    ///
    /// Las ventas offline son el caso m�s cr�tico: el stock ya fue descontado localmente
    /// pero Supabase a�n no lo sabe. Al sincronizar, el SyncService aplica el INSERT
    /// de la venta en Supabase. Si el stock en Supabase ya no alcanza (otra caja vendi�
    /// el mismo producto), Supabase retorna error y SyncService lo marca como MarkFailed.
    /// El operador resuelve manualmente desde el panel de admin de Blazor.
    /// </summary>
    public class RoutingVentaRepository : IVentaRepository
    {
        private readonly IConnectivityService _connectivity;
        private readonly ISyncOutboxRepository _outbox;
        private readonly InventarioDbContext _pgContext;
        private readonly InventarioDbContext _localContext;
        private readonly string _deviceId;

        private VentaRepository PgRepo => new(_pgContext);
        private VentaRepository LocalRepo => new(_localContext);

        public RoutingVentaRepository(
            IConnectivityService connectivity,
            ISyncOutboxRepository outbox,
            InventarioDbContext pgContext,
            InventarioDbContext localContext,
            string deviceId)
        {
            _connectivity = connectivity;
            _outbox = outbox;
            _pgContext = pgContext;
            _localContext = localContext;
            _deviceId = deviceId;
        }

        public async Task<int> AddAsync(Venta venta)
        {
            if (_connectivity.IsOnline)
            {
                var id = await PgRepo.AddAsync(venta);
                // Replicar en local para tener historial offline
                try { await LocalRepo.AddAsync(venta); } catch { /* best-effort */ }
                return id;
            }

            // Offline: guardar local + encolar para Supabase
            var localId = await LocalRepo.AddAsync(venta);

            await _outbox.EnqueueAsync(new SyncOutboxEntry
            {
                OperationType = "INSERT",
                EntityTable   = "sale",
                EntityId      = localId,
                PayloadJson   = JsonSerializer.Serialize(venta),
                DeviceId      = _deviceId
            });

            // Encolar tambi�n los detalles
            foreach (var detalle in venta.Detalles)
            {
                await _outbox.EnqueueAsync(new SyncOutboxEntry
                {
                    OperationType = "INSERT",
                    EntityTable   = "sale_detail",
                    EntityId      = null,
                    PayloadJson   = JsonSerializer.Serialize(detalle),
                    DeviceId      = _deviceId
                });
            }

            return localId;
        }

        public Task<Venta?> GetByIdAsync(int id)
            => _connectivity.IsOnline
                ? PgRepo.GetByIdAsync(id)
                : LocalRepo.GetByIdAsync(id);

        public Task<IReadOnlyList<Venta>> GetByFechaAsync(DateTime? desde, DateTime? hasta)
            => _connectivity.IsOnline
                ? PgRepo.GetByFechaAsync(desde, hasta)
                : LocalRepo.GetByFechaAsync(desde, hasta);
    }
}