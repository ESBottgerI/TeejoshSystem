using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
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
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo, cancellationToken);

            if (usuario is null)
            {
                BCrypt.Net.BCrypt.Verify(password, PlaceholderHash);
                return AutenticacionResultado.Invalido("Usuario o contraseña incorrectos.");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                return AutenticacionResultado.Invalido("Usuario o contraseña incorrectos.");

            return AutenticacionResultado.Valido(usuario.Id, usuario.NombreUsuario, usuario.Rol);
        }

        public async Task<bool> VerificarPasswordAsync(
            int usuarioId,
            string password,
            CancellationToken cancellationToken = default)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Activo, cancellationToken);

            if (usuario is null) return false;

            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }
    }
}