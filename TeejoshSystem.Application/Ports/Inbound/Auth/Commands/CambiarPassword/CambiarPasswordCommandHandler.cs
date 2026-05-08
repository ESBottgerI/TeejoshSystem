using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword
{
    public class CambiarPasswordCommandHandler : IRequestHandler<CambiarPasswordCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;

        public CambiarPasswordCommandHandler(IAuthService authService, IUsuarioRepository usuarioRepository)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Result> Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PasswordNuevo) || request.PasswordNuevo.Length < 8)
                    return Result.Failure("La contraseña nueva debe tener al menos 8 caracteres.");

                if (request.PasswordActual == request.PasswordNuevo)
                    return Result.Failure("La contraseña nueva debe ser distinta a la actual.");

                var passwordValida = await _authService.VerificarPasswordAsync(
                    request.UsuarioId, request.PasswordActual, cancellationToken);

                if (!passwordValida)
                    return Result.Failure("La contraseña actual es incorrecta.");

                await _usuarioRepository.ActualizarPasswordAsync(
                    request.UsuarioId, request.PasswordNuevo, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure("Error al cambiar la contraseña.");
            }
        }
    }
}