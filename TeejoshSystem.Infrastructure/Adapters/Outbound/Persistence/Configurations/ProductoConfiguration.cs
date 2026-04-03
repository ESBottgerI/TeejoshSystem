using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("product");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");

            // Value Object: Nombre
            builder.OwnsOne(p => p.Nombre, n =>
            {
                n.Property(x => x.Value)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Value Object: Precio
            builder.OwnsOne(p => p.Precio, p =>
            {
                p.Property(x => x.Value)
                    .HasColumnName("price")
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();
            });

            // Value Object: Stock
            builder.OwnsOne(p => p.Stock, u =>
            {
                u.Property(x => x.Value)
                    .HasColumnName("units")
                    .IsRequired();
            });

            //builder.Ignore(p => p.Tipo);
            /*
            builder.Property(p => p.Tipo)
                .HasConversion<int>()
                .HasColumnName("tipo_producto");
            */

            builder.Ignore(p => p.Descripcion);

            /*
            builder.HasOne(p => p.Descripcion)
                .WithOne()
                .HasForeignKey<ProductoDetalle>(d => d.ProductoId)
                .IsRequired();
            */
        }
    }
}
