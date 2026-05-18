using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario
{
    public class AutenticarUsuarioCommandHandler
        : IRequestHandler<AutenticarUsuarioCommand, Result<SesionDto>>
    {
        private readonly IAuthService _authService;
        private readonly IAppLogger _logger;                 // NUEVO

        public AutenticarUsuarioCommandHandler(
            IAuthService authService,
            IAppLogger logger)                               // NUEVO
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<Result<SesionDto>> Handle(
            AutenticarUsuarioCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                    return Result.Failure<SesionDto>("El nombre de usuario es obligatorio.");

                if (string.IsNullOrWhiteSpace(request.Password))
                    return Result.Failure<SesionDto>("La contraseña es obligatoria.");

                var resultado = await _authService.AutenticarAsync(
                    request.NombreUsuario.Trim(),
                    request.Password,
                    cancellationToken);

                if (!resultado.Exitoso)
                {
                    // Loguear intento fallido sin exponer la contraseña
                    _logger.Warning($"Fallo de autenticación para usuario '{request.NombreUsuario}': {resultado.MensajeError}");
                    return Result.Failure<SesionDto>(
                        resultado.MensajeError ?? "Credenciales inválidas.");
                }

                _logger.Info($"Autenticación exitosa: usuario='{resultado.NombreUsuario}', rol={resultado.Rol}");
                return Result.Success(
                    new SesionDto(resultado.UsuarioId!.Value, resultado.NombreUsuario!, resultado.Rol!.Value));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al autenticar usuario '{request.NombreUsuario}'", ex);
                return Result.Failure<SesionDto>("Error al autenticar. Intente nuevamente.");
            }
        }
    }
}