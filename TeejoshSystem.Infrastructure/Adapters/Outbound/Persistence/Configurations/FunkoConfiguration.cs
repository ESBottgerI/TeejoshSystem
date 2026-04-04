using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class FunkoConfiguration : IEntityTypeConfiguration<FunkoDetalle>
    {
        public void Configure(EntityTypeBuilder<FunkoDetalle> builder)
        {
            builder.HasBaseType((Type)null);

            builder.ToTable("funko");

            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.NumeroCaja)
                .HasColumnName("box_number")
                .IsRequired();

            builder.Property(d => d.Licencia)
                .HasColumnName("license")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.SubtipoId)
                .HasColumnName("subtype")
                .IsRequired();

            builder.Property(d => d.CaracteristicaEspecialId)
                .HasColumnName("special_caracteristic")
                .IsRequired();

            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<FunkoDetalle>(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
