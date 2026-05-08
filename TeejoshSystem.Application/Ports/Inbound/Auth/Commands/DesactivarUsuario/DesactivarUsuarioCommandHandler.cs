using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario
{
    public class DesactivarUsuarioCommandHandler : IRequestHandler<DesactivarUsuarioCommand, Result>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public DesactivarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Result> Handle(DesactivarUsuarioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _usuarioRepository.DesactivarAsync(request.UsuarioId, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure("Error al desactivar el usuario.");
            }
        }
    }
}