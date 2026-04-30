using MediatR;

using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto
{
    public class EliminarProductoCommandHandler
        : IRequestHandler<EliminarProductoCommand, Result>
    {
        private readonly IProductoRepository _repository;

        public EliminarProductoCommandHandler(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(
            EliminarProductoCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
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
