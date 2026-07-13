using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    /// <summary>
    /// DbContext exclusivo para SQLite local.
    /// Extiende InventarioDbContext agregando la tabla sync_outbox.
    ///
    /// Usa el constructor protegido no-genérico de InventarioDbContext
    /// para poder pasar DbContextOptions<LocalDbContext> a la clase base.
    /// Este es el patrón oficial de EF Core para herencia de DbContext.
    /// </summary>
    public class LocalDbContext : InventarioDbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<SyncOutboxEntry> SyncOutbox => Set<SyncOutboxEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SyncOutboxEntryConfiguration());
        }
    }
}
