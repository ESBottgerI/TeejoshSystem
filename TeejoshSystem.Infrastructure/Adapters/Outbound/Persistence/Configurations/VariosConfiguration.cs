using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class VariosConfiguration : IEntityTypeConfiguration<VariosDetalle>
    {
        public void Configure(EntityTypeBuilder<VariosDetalle> builder)
        {
            builder.HasBaseType((Type)null);

            builder.ToTable("varios");

            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.Marca)
                .HasColumnName("brand")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Alto)
                .HasColumnName("height")
                .IsRequired();

            builder.Property(d => d.Ancho)
                .HasColumnName("width")
                .IsRequired();

            builder.Property(d => d.Largo)
                .HasColumnName("length")
                .IsRequired();

            builder.Property(d => d.Material)
                .HasColumnName("material")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.TieneIlustracion)
                .HasColumnName("ilustration")
                .IsRequired();

            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<VariosDetalle>(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}