using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class HotWheelsConfiguration : IEntityTypeConfiguration<HotWheelsDetalle>
    {
        public void Configure(EntityTypeBuilder<HotWheelsDetalle> builder)
        {
            builder.HasBaseType((Type)null);

            builder.ToTable("hot_wheels");

            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.Modelo)
                .HasColumnName("model")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Anio)
                .HasColumnName("year")
                .IsRequired();

            builder.Property(d => d.Serie)
                .HasColumnName("serie")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.CategoriaId)
                .HasColumnName("category_id")
                .IsRequired();

            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<HotWheelsDetalle>(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
