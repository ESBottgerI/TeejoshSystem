namespace TeejoshSystem.Application.Common.Dtos
{
    /// <summary>
    /// DTO de sesión activa. Cruza de Application hacia UI.
    /// Nunca cruza hacia Domain.
    /// </summary>
    public record SesionDto(int UsuarioId, string NombreUsuario);
}