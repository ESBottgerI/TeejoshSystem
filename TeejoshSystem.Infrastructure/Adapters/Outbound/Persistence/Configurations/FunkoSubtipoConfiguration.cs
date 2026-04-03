using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Catalogos;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class FunkoSubtipoConfiguration : IEntityTypeConfiguration<FunkoSubtipo>
    {
        public void Configure(EntityTypeBuilder<FunkoSubtipo> builder)
        {
            builder.ToTable("funko_subtype");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasColumnName("id");

            builder.Property(s => s.Nombre)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(s => s.Nombre).IsUnique();
        }
    }
}