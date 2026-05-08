using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios
{
    public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, IEnumerable<UsuarioListaDto>>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<UsuarioListaDto>> Handle(
            ListarUsuariosQuery request,
            CancellationToken cancellationToken)
        {
            var usuarios = await _usuarioRepository.ListarAsync(cancellationToken);
            return usuarios.Select(u => new UsuarioListaDto(u.Id, u.NombreUsuario, u.Rol, u.Activo));
        }
    }
}