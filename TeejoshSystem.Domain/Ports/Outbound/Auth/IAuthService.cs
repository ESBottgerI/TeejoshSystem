using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Domain.Ports.Outbound.Auth
{
    public interface IAuthService
    {
        Task<AutenticacionResultado> AutenticarAsync(
            string nombreUsuario,
            string password,
            CancellationToken cancellationToken = default);

        // Necesario para CambiarPassword — verifica sin conocer el username
        Task<bool> VerificarPasswordAsync(
            int usuarioId,
            string password,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Resultado de valor del proceso de autenticación.
    /// Definido en Domain porque es un concepto del dominio, no un DTO de Application.
    /// No usa Result<T> de Application — Domain no depende de Application.
    /// </summary>
    
    public record AutenticacionResultado(
        bool Exitoso,
        int? UsuarioId,
        string? NombreUsuario,
        RolUsuario? Rol,
        string? MensajeError)
    {
        public static AutenticacionResultado Valido(int usuarioId, string nombreUsuario, RolUsuario rol)
            => new(true, usuarioId, nombreUsuario, rol, null);

        public static AutenticacionResultado Invalido(string motivo)
            => new(false, null, null, null, motivo);
    }
}