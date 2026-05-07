using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Auth
{
    public class LocalAuthService : IAuthService
    {
        private readonly InventarioDbContext _context;

        // Hash de placeholder para timing attack mitigation.
        // BCrypt.Verify contra un hash válido tarda ~100-300ms independientemente del resultado.
        // Sin esto, "usuario no existe" retorna en microsegundos y "contraseña incorrecta"
        // tarda 300ms — un atacante puede enumerar usuarios midiendo tiempos de respuesta.
        private const string PlaceholderHash =
            "$2a$12$placeholderHashParaMitigacionDeTimingXXXXXXXXXXXXXXXX";

        public LocalAuthService(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task<AutenticacionResultado> AutenticarAsync(
            string nombreUsuario,
            string password,
            CancellationToken cancellationToken = default)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.NombreUsuario == nombreUsuario && u.Activo,
                    cancellationToken);

            if (usuario is null)
            {
                // Verificar contra hash falso para equiparar tiempo de respuesta.
                BCrypt.Net.BCrypt.Verify(password, PlaceholderHash);
                return AutenticacionResultado.Invalido("Usuario o contraseña incorrectos.");
            }

            var passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            // El mensaje es idéntico para usuario inexistente y contraseña incorrecta.
            // Mensajes distintos permitirían enumerar usuarios válidos.
            if (!passwordValida)
                return AutenticacionResultado.Invalido("Usuario o contraseña incorrectos.");

            return AutenticacionResultado.Valido(usuario.Id, usuario.NombreUsuario);
        }
    }
}