using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class FunkoConfiguration : IEntityTypeConfiguration<FunkoDetalle>
    {
        public void Configure(EntityTypeBuilder<FunkoDetalle> builder)
        {
            // ✅ Tratar como entidad independiente, NO como herencia
            builder.HasBaseType((Type)null);

            builder.ToTable("funko");

            // ✅ Primary Key
            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.NumeroCaja)
                .HasColumnName("box_number")
                .IsRequired();

            builder.Property(d => d.Licencia)
                .HasColumnName("license")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(d => d.SubtipoId)
                .HasColumnName("subtype_id")
                .IsRequired();

            builder.Property(d => d.CaracteristicaEspecialId)
                .HasColumnName("special_feature_id");
        }
    }
}
