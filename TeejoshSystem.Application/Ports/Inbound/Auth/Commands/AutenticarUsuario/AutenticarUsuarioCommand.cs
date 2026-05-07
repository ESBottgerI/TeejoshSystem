using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario
{
    public class AutenticarUsuarioCommand : IRequest<Result<SesionDto>>
    {
        public string NombreUsuario { get; }
        public string Password { get; }

        public AutenticarUsuarioCommand(string nombreUsuario, string password)
        {
            NombreUsuario = nombreUsuario;
            Password = password;
        }
    }
}