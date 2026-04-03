using MediatR;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;
using TeejoshInventario.Application.Common;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Commands.EliminarProducto
{
    public class EliminarProductosCommandHandler
        : IRequestHandler<EliminarProductosCommand, Result>
    {
        private readonly IProductoRepository _repository;

        public EliminarProductosCommandHandler(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(
            EliminarProductosCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que existan todos los productos
                foreach (var id in request.ProductoIds)
                {
                    if (!await _repository.ExistsAsync(id))
                        return Result.Failure($"Producto con ID {id} no encontrado");
                }

                // Eliminar
                await _repository.DeleteRangeAsync(request.ProductoIds);

                return Result.Success();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                return Result.Failure("Error al eliminar productos");
            }
        }
    }
}
