using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgConfiguration : IEntityTypeConfiguration<TcgDetalle>
    {
        public void Configure(EntityTypeBuilder<TcgDetalle> builder)
        {
            builder.HasBaseType((Type)null);

            builder.ToTable("tcg");

            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.PackId)
                .HasColumnName("pack_id")
                .IsRequired();

            builder.Property(d => d.ExpansionId)
                .HasColumnName("expansion_id")
                .IsRequired();

            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<TcgDetalle>(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
