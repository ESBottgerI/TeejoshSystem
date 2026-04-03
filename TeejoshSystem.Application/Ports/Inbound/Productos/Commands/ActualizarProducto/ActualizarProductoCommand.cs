using MediatR;
using TeejoshInventario.Application.Common;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Commands.ActualizarProducto
{
    public record ActualizarProductoCommand(
        int Id,
        string Nombre,
        decimal Precio,
        int Unidades
    ) : IRequest<Result>;
}
