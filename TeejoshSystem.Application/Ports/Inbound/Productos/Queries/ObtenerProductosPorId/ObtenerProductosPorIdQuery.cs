using MediatR;

using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId
{
    public class ObtenerProductosPorIdQuery : IRequest<IReadOnlyList<ProductoDto>> { }
}
