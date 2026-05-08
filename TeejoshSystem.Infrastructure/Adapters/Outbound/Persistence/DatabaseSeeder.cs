using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence
{
    public static class DatabaseSeeder
    {
        /// <summary>
        /// Crea el usuario admin solo si no existe ningún usuario en la BD.
        /// Este seed es temporal — existe para que el sistema sea accesible
        /// en instalación nueva hasta que se implemente la gestión de usuarios.
        /// </summary>
        public static void SeedUsuarioAdmin(InventarioDbContext db)
        {
            if (db.Usuarios.Any()) return;

            db.Database.ExecuteSqlRaw(
                "INSERT INTO app_user (username, password_hash, rol, active) VALUES ('admin', {0}, 'Administrador', 1)",
                BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12));
        }
    }
}