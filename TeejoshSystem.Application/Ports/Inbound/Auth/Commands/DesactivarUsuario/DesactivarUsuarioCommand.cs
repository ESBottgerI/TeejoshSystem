using MediatR;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario
{
    public class DesactivarUsuarioCommand : IRequest<Result>
    {
        public int UsuarioId { get; }
        public DesactivarUsuarioCommand(int usuarioId) => UsuarioId = usuarioId;
    }
}