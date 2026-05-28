

using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; private set; }
        public RolUsuario Rol { get; private set; }
        public string NombreUsuario { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public bool Activo { get; private set; }

        internal Usuario() { } // Para EF Core
    }
}