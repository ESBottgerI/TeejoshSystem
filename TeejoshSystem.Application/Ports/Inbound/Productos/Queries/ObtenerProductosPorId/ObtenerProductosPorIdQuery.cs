using MediatR;

using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId
{
    public class ObtenerProductosPorIdQuery : IRequest<ProductoDetalladoDto>
    {
        public int Id { get; }

        public ObtenerProductosPorIdQuery(int id)
        {
            Id = id;
        }
    }
}