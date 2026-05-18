using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, Result>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IAppLogger _logger;                 // NUEVO

        public RegistrarUsuarioCommandHandler(
            IUsuarioRepository usuarioRepository,
            IAppLogger logger)                               // NUEVO
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                    return Result.Failure("El nombre de usuario es obligatorio.");

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                    return Result.Failure("La contraseña debe tener al menos 8 caracteres.");

                if (await _usuarioRepository.ExisteAsync(request.NombreUsuario, cancellationToken))
                {
                    _logger.Warning($"Intento de registrar usuario duplicado: '{request.NombreUsuario}'");
                    return Result.Failure("El nombre de usuario ya está en uso.");
                }

                await _usuarioRepository.CrearAsync(request.NombreUsuario, request.Password, request.Rol, cancellationToken);

                _logger.Info($"Usuario registrado exitosamente: '{request.NombreUsuario}', Rol={request.Rol}");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inesperado al registrar usuario '{request.NombreUsuario}'", ex);
                return Result.Failure("Error al registrar el usuario.");
            }
        }
    }
}