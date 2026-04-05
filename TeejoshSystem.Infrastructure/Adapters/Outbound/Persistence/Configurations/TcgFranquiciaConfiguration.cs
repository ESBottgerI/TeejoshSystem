using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeejoshSystem.Domain.Entities.Catalogos;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class TcgFranquiciaConfiguration : IEntityTypeConfiguration<TcgFranquicia>
    {
        public void Configure(EntityTypeBuilder<TcgFranquicia> builder)
        {
            builder.ToTable("tcg_franchise");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");

            builder.Property(p => p.Nombre)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(p => p.Nombre)
                .IsUnique();
        }
    }
}