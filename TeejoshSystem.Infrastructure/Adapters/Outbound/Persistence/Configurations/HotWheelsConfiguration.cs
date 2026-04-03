using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities;
using TeejoshInventario.Domain.Entities.Detalles;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class HotWheelsConfiguration : IEntityTypeConfiguration<HotWheelsDetalle>
    {
        public void Configure(EntityTypeBuilder<HotWheelsDetalle> builder)
        {
            // ✅ Tratar como entidad independiente, NO como herencia
            builder.HasBaseType((Type)null);

            builder.ToTable("hot_wheels");

            // ✅ Primary Key
            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.Modelo)
                .HasColumnName("model")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.Anio)
                .HasColumnName("year")
                .IsRequired();

            builder.Property(d => d.Serie)
                .HasColumnName("serie")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.CategoriaId)
                .HasColumnName("category_id")
                .IsRequired();
        }
    }
}
