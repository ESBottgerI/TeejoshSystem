using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class FunkoCaracteristicaConfiguration : IEntityTypeConfiguration<FunkoCaracteristica>
    {
        public void Configure(EntityTypeBuilder<FunkoCaracteristica> builder)
        {
            builder.ToTable("funko_special_feature");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("id");

            builder.Property(c => c.Nombre)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(c => c.Nombre).IsUnique();
        }
    }
}