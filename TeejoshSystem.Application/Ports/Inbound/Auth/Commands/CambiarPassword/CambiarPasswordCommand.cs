using MediatR;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword
{
    public class CambiarPasswordCommand : IRequest<Result>
    {
        public int UsuarioId { get; }
        public string PasswordActual { get; }
        public string PasswordNuevo { get; }

        public CambiarPasswordCommand(int usuarioId, string passwordActual, string passwordNuevo)
        {
            UsuarioId = usuarioId;
            PasswordActual = passwordActual;
            PasswordNuevo = passwordNuevo;
        }
    }
}