using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Catalogos;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgPackConfiguration : IEntityTypeConfiguration<TcgPack>
    {
        public void Configure(EntityTypeBuilder<TcgPack> builder)
        {
            builder.ToTable("tcg_pack");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasColumnName("id");

            builder.Property(p => p.Nombre)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.FranquiciaId)
                .HasColumnName("franchise_id")
                .IsRequired();

            builder.HasIndex(p => new { p.Nombre, p.FranquiciaId }).IsUnique();
        }
    }
}