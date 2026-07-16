using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Auth;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Auth
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly InventarioDbContext _context;

        public UsuarioRepository(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteAsync(string nombreUsuario, CancellationToken ct = default)
            => await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario, ct);

        public async Task CrearAsync(string nombreUsuario, string passwordPlano, RolUsuario rol, CancellationToken ct = default)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(passwordPlano, workFactor: 12);
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO app_user (username, password_hash, rol, active) VALUES ({0}, {1}, {2}, true)",
                nombreUsuario, hash, rol.ToString());
        }

        public async Task<IEnumerable<Usuario>> ListarAsync(CancellationToken ct = default)
            => await _context.Usuarios.AsNoTracking().ToListAsync(ct);

        public async Task ActualizarPasswordAsync(int usuarioId, string passwordNuevo, CancellationToken ct = default)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(passwordNuevo, workFactor: 12);
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE app_user SET password_hash = {0} WHERE id = {1}",
                hash, usuarioId);
        }

        public async Task DesactivarAsync(int usuarioId, CancellationToken ct = default)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE app_user SET active = false WHERE id = {0}", usuarioId);
        }
    }
}