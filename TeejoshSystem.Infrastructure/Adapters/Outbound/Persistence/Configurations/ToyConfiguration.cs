using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshInventario.Domain.Entities.Detalles;

namespace TeejoshInventario.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class ToyConfiguration : IEntityTypeConfiguration<ToyDetalle>
    {
        public void Configure(EntityTypeBuilder<ToyDetalle> builder)
        {
            // ✅ Tratar como entidad independiente, NO como herencia
            builder.HasBaseType((Type)null);

            builder.ToTable("toy");

            // ✅ Primary Key
            builder.HasKey(d => d.ProductoId);

            builder.Property(d => d.ProductoId)
                .HasColumnName("product_id");

            builder.Property(d => d.EdadMinima)
                .HasColumnName("min_years")
                .IsRequired();

            builder.Property(d => d.JugadoresMin)
                .HasColumnName("min_players")
                .IsRequired();

            builder.Property(d => d.JugadoresMax)
                .HasColumnName("max_players")
                .IsRequired();

            builder.Property(d => d.EsJuegoDeMesa)
                .HasColumnName("board_game")
                .IsRequired();
        }
    }
}