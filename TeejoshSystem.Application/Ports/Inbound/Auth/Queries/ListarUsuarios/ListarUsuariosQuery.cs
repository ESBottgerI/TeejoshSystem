using System.Collections.Generic;
using MediatR;
using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios
{
    public class ListarUsuariosQuery : IRequest<IEnumerable<UsuarioListaDto>> { }
}