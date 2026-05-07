namespace TeejoshSystem.Domain.Ports.Outbound.Auth
{
    public interface IAuthService
    {
        Task<AutenticacionResultado> AutenticarAsync(
            string nombreUsuario,
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
        string? MensajeError)
    {
        public static AutenticacionResultado Valido(int usuarioId, string nombreUsuario)
            => new(true, usuarioId, nombreUsuario, null);

        public static AutenticacionResultado Invalido(string motivo)
            => new(false, null, null, motivo);
    }
}