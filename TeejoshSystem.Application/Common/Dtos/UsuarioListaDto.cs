using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Application.Common.Dtos
{
    public record UsuarioListaDto(int Id, string NombreUsuario, RolUsuario Rol, bool Activo);
}