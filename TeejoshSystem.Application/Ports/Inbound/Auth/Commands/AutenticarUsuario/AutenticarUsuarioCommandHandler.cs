using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario
{
    public class AutenticarUsuarioCommandHandler
        : IRequestHandler<AutenticarUsuarioCommand, Result<SesionDto>>
    {
        private readonly IAuthService _authService;

        public AutenticarUsuarioCommandHandler(IAuthService authService)
        {
            _authService = authService;
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
                    return Result.Failure<SesionDto>(
                        resultado.MensajeError ?? "Credenciales inválidas.");

                return Result.Success(
                    new SesionDto(resultado.UsuarioId!.Value, resultado.NombreUsuario!, resultado.Rol!.Value));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure<SesionDto>("Error al autenticar. Intente nuevamente.");
            }
        }
    }
}