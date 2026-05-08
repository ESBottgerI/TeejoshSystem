using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommand : IRequest<Result>
    {
        public string NombreUsuario { get; }
        public string Password { get; }
        public RolUsuario Rol { get; }

        public RegistrarUsuarioCommand(string nombreUsuario, string password, RolUsuario rol)
        {
            NombreUsuario = nombreUsuario;
            Password = password;
            Rol = rol;
        }
    }
}