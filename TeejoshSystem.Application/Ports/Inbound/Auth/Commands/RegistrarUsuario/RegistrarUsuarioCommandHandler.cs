using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario
{
    public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, Result>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public RegistrarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
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
                    return Result.Failure("El nombre de usuario ya está en uso.");

                await _usuarioRepository.CrearAsync(request.NombreUsuario, request.Password, request.Rol, cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return Result.Failure("Error al registrar el usuario.");
            }
        }
    }
}