using MediatR;
using TeejoshInventario.Domain.Ports.Outbound.Repositories;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Queries.BuscarProductos
{
    public class BuscarProductosQueryHandler : IRequestHandler<BuscarProductosQuery, List<ProductoBusquedaDto>>
    {
        private readonly IProductoRepository _repository;

        public BuscarProductosQueryHandler(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductoBusquedaDto>> Handle(
            BuscarProductosQuery request,
            CancellationToken cancellationToken)
        {
            var productos = await _repository.SearchAsync(request.Nombre, request.Tipo);

            return productos.Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Nombre = p.Nombre.Value,
                Precio = p.Precio.Value,
                Unidades = p.Stock.Value,
                Tipo = "Producto", // TODO: Obtener tipo del producto
                DetalleResumen = "" // TODO: Generar resumen segun tipo
            }).ToList();
        }
    }
}
