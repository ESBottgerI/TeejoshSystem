using MediatR;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos
{
    public class ObtenerProductosQuery : IRequest<IReadOnlyList<ProductoDto>>
    {
    }
}
