using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("audit_log");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasColumnName("id");

            builder.Property(a => a.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            builder.Property(a => a.Usuario)
                .HasColumnName("usuario")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(a => a.Entidad)
                .HasColumnName("entidad")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.EntidadId)
                .HasColumnName("entidad_id")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.Accion)
                .HasColumnName("accion")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.Cambios)
                .HasColumnName("cambios")
                .HasColumnType("TEXT")
                .IsRequired(false);

            builder.HasIndex(a => a.Timestamp);
            builder.HasIndex(a => new { a.Entidad, a.EntidadId });
        }
    }
}