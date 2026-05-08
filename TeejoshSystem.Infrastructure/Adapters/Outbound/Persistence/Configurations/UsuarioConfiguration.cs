using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeejoshSystem.Domain.Entities;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("app_user");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("id");

            builder.Property(u => u.Rol)
                .HasColumnName("rol")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(u => u.NombreUsuario)
                .HasColumnName("username")
                .HasMaxLength(100)
                .IsRequired();

            // BCrypt siempre produce exactamente 60 caracteres.
            // Si este valor cambia, el algoritmo de hash cambió — revisar LocalAuthService.
            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(u => u.Activo)
                .HasColumnName("active")
                .HasDefaultValue(true);

            // Unicidad garantizada en BD, no solo en aplicación.
            builder.HasIndex(u => u.NombreUsuario)
                .IsUnique();
        }
    }
}