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

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.Nombre)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(e => e.Nombre)
                .IsUnique();
        }
    }
}