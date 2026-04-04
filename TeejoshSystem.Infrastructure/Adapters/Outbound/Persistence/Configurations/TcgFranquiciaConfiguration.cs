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

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .HasColumnName("id");

            builder.Property(f => f.Nombre)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(f => f.Nombre)
                .IsUnique();
        }
    }
}