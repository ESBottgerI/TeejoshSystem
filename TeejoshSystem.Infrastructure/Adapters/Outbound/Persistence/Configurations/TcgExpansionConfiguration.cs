using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Catalogos;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgExpansionConfiguration : IEntityTypeConfiguration<TcgExpansion>
    {
        public void Configure(EntityTypeBuilder<TcgExpansion> builder)
        {
            builder.ToTable("tcg_expansion");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");

            builder.Property(e => e.Nombre)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.FranquiciaId)
                .HasColumnName("franchise_id")
                .IsRequired();

            builder.HasIndex(e => new { e.Nombre, e.FranquiciaId }).IsUnique();
        }
    }
}