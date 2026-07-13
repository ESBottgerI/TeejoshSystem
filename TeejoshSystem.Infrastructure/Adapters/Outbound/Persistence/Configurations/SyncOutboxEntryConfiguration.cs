using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    /// <summary>
    /// Configuración EF Core para la tabla sync_outbox.
    /// Esta tabla existe SOLO en SQLite local — nunca se migra a Supabase.
    ///
    /// La tabla almacena operaciones de escritura realizadas offline
    /// que serán replicadas en Supabase al reconectar.
    /// </summary>
    public class SyncOutboxEntryConfiguration : IEntityTypeConfiguration<SyncOutboxEntry>
    {
        public void Configure(EntityTypeBuilder<SyncOutboxEntry> builder)
        {
            builder.ToTable("sync_outbox");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever(); // El GUID lo genera el dominio, no SQLite

            builder.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.EntityTable)
                .HasColumnName("entity_table")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.EntityId)
                .HasColumnName("entity_id");

            builder.Property(e => e.PayloadJson)
                .HasColumnName("payload_json")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(e => e.DeviceId)
                .HasColumnName("device_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.RetryCount)
                .HasColumnName("retry_count")
                .HasDefaultValue(0);

            builder.Property(e => e.LastError)
                .HasColumnName("last_error");

            // Índice para acelerar la consulta de pendientes (los más frecuentes)
            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_outbox_created_at");
        }
    }
}