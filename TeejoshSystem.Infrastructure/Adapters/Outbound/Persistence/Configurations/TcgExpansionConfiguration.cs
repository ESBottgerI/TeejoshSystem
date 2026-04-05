using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgExpansionConfiguration : IEntityTypeConfiguration<TcgExpansion>
    {
        public void Configure(EntityTypeBuilder<TcgExpansion> builder)
        {
            builder.ToTable("tcg_expansion");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");

            builder.Property(p => p.Nombre)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(p => p.Nombre)
                .IsUnique();
            
            builder.HasIndex(p => new { p.Nombre, p.FranquiciaId })
                .IsUnique();
        }
    }
}