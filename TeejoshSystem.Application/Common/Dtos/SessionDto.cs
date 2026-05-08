using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Application.Common.Dtos
{
    public record SesionDto(int UsuarioId, string NombreUsuario, RolUsuario Rol);
}