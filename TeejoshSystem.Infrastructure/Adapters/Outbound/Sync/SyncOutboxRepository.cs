using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Sync
{
    /// <summary>
    /// Implementacion del outbox sobre SQLite local.
    /// Siempre usa LocalDbContext - nunca escribe en Supabase.
    /// </summary>
    public class SyncOutboxRepository : ISyncOutboxRepository
    {
        private readonly LocalDbContext _db;

        public SyncOutboxRepository(LocalDbContext db)
        {
            _db = db;
        }

        public async Task EnqueueAsync(SyncOutboxEntry entry, CancellationToken ct = default)
        {
            await _db.SyncOutbox.AddAsync(entry, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<SyncOutboxEntry>> GetPendingAsync(CancellationToken ct = default)
        {
            return await _db.SyncOutbox
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task MarkSyncedAsync(Guid entryId, CancellationToken ct = default)
        {
            var entry = await _db.SyncOutbox.FindAsync(new object[] { entryId }, ct);
            if (entry is null) return;

            _db.SyncOutbox.Remove(entry);
            await _db.SaveChangesAsync(ct);
        }

        public async Task MarkFailedAsync(Guid entryId, string error, CancellationToken ct = default)
        {
            var entry = await _db.SyncOutbox.FindAsync(new object[] { entryId }, ct);
            if (entry is null) return;

            entry.RetryCount++;
            entry.LastError = error;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<int> CountPendingAsync(CancellationToken ct = default)
        {
            return await _db.SyncOutbox.CountAsync(ct);
        }
    }
}