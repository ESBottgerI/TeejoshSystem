using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Domain.Ports.Outbound.Auth
{
    public interface IUsuarioRepository
    {
        Task<bool> ExisteAsync(string nombreUsuario, CancellationToken ct = default);
        Task CrearAsync(string nombreUsuario, string passwordPlano, RolUsuario rol, CancellationToken ct = default);
        Task<IEnumerable<Usuario>> ListarAsync(CancellationToken ct = default);
        Task ActualizarPasswordAsync(int usuarioId, string passwordNuevo, CancellationToken ct = default);
        Task DesactivarAsync(int usuarioId, CancellationToken ct = default);
    }
}