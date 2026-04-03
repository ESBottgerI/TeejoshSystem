using MediatR;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Queries.ObtenerProductos
{
    public class ObtenerProductosQuery : IRequest<IReadOnlyList<ProductoDto>>
    {
    }
}
