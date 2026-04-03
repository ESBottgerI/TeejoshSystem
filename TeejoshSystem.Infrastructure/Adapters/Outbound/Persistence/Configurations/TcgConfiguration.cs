using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Detalles;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgConfiguration : IEntityTypeConfiguration<TcgDetalle>
    {
        public void Configure(EntityTypeBuilder<TcgDetalle> builder)
        {
            // ✅ Tratar como entidad independiente, NO como herencia
            builder.HasBaseType((Type)null);

            builder.ToTable("tcg");

            // ✅ Primary Key
            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.PackId)
                .HasColumnName("pack_id")
                .IsRequired();

            builder.Property(d => d.ExpansionId)
                .HasColumnName("expansion_id")
                .IsRequired();
        }
    }
}
