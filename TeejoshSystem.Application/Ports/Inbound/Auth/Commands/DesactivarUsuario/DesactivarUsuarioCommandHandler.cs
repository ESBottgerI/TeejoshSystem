using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario
{
    public class DesactivarUsuarioCommandHandler : IRequestHandler<DesactivarUsuarioCommand, Result>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAppLogger _logger;                 // NUEVO

        public DesactivarUsuarioCommandHandler(
            IUsuarioRepository usuarioRepository,
            IAppLogger logger)                               // NUEVO
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(DesactivarUsuarioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _usuarioRepository.DesactivarAsync(request.UsuarioId, cancellationToken);
                _logger.Info($"Usuario desactivado: UsuarioId={request.UsuarioId}");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al desactivar usuario: UsuarioId={request.UsuarioId}", ex);
                return Result.Failure("Error al desactivar el usuario.");
            }
        }
    }
}