using MediatR;
using TeejoshInventario.Application.Common;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Commands.EliminarProducto
{
    public record EliminarProductosCommand(List<int> ProductoIds) : IRequest<Result>;
}
