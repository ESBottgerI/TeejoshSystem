using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Detalles;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class VariosConfiguration : IEntityTypeConfiguration<VariosDetalle>
    {
        public void Configure(EntityTypeBuilder<VariosDetalle> builder)
        {
            // ✅ Tratar como entidad independiente, NO como herencia
            builder.HasBaseType((Type)null);

            builder.ToTable("varios");

            // ✅ Primary Key
            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.Marca)
                .HasColumnName("brand")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.Alto)
                .HasColumnName("height")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(d => d.Ancho)
                .HasColumnName("width")
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(d => d.Largo)
                .HasColumnName("length")
                .HasColumnType("decimal(5,2)");

            builder.Property(d => d.Material)
                .HasColumnName("material")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.TieneIlustracion)
                .HasColumnName("illustration")
                .IsRequired();
        }
    }
}