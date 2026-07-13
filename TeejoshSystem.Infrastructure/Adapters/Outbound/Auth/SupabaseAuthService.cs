using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Auth
{
    /// <summary>
    /// Implementación de IAuthService para el proveedor PostgreSQL (Supabase).
    /// Autentica contra la tabla app_user gestionada por EF Core — no usa GoTrue,
    /// que está diseñado para flujos web/OAuth, no para usuarios internos de desktop.
    /// La lógica de BCrypt y timing-attack mitigation es idéntica a LocalAuthService.
    /// </summary>
    public class SupabaseAuthService : IAuthService
    {
        private readonly InventarioDbContext _context;

        // Hash de placeholder para mitigación de timing attack.
        // Sin esto, "usuario no existe" retorna en microsegundos y "contraseña incorrecta"
        // tarda ~300ms — un atacante puede enumerar usuarios midiendo tiempos de respuesta.
        private const string PlaceholderHash =
            "$2a$12$placeholderHashParaMitigacionDeTimingXXXXXXXXXXXXXXXX";

        public SupabaseAuthService(InventarioDbContext context)
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
                // Consumir tiempo equivalente para no revelar si el usuario existe.
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
                .FirstOrDefaultAsync(
                    u => u.Id == usuarioId && u.Activo,
                    cancellationToken);

            if (usuario is null) return false;

            return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        }
    }
}