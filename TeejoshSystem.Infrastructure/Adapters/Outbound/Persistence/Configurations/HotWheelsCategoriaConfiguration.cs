using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class HotWheelsCategoriaConfiguration : IEntityTypeConfiguration<HotWheelsCategoria>
    {
        public void Configure(EntityTypeBuilder<HotWheelsCategoria> builder)
        {
            builder.ToTable("hot_wheels_category");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("id");

            builder.Property(c => c.Nombre)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(c => c.Nombre)
                .IsUnique();
        }
    }
}
