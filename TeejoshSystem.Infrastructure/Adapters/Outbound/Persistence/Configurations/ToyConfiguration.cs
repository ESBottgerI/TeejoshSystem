using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class ToyConfiguration : IEntityTypeConfiguration<ToyDetalle>
    {
        public void Configure(EntityTypeBuilder<ToyDetalle> builder)
        {
            builder.HasBaseType((Type)null);

            builder.ToTable("toy");

            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.EdadMinima)
                .HasColumnName("min_years_old")
                .IsRequired();

            builder.Property(d => d.JugadoresMin)
                .HasColumnName("min_players")
                .IsRequired();

            builder.Property(d => d.JugadoresMax)
                .HasColumnName("max_players")
                .IsRequired();

            builder.Property(d => d.EsJuegoDeMesa)
                .HasColumnName("is_board_game")
                .IsRequired();

            builder.HasOne<Producto>()
                .WithOne()
                .HasForeignKey<ToyDetalle>(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}