namespace TeejoshSystem.Domain.Ports.Outbound
{
    /// <summary>
    /// Permite a Infrastructure conocer el usuario activo sin depender de la UI.
    /// </summary>
    public interface ICurrentUserProvider
    {
        string? UsuarioActual { get; }
    }
}