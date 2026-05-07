// Archivo: TeejoshSystem.Application/Ports/Inbound/Productos/Queries/BuscarProductos/BuscarProductosQueryHandler.cs
using MediatR;

using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos
{
    public class BuscarProductosQueryHandler : IRequestHandler<BuscarProductosQuery, List<ProductoBusquedaDto>>
    {
        private readonly IProductoRepository _repository;
        private readonly IImageStorageService _imageStorage;  // NUEVO

        public BuscarProductosQueryHandler(
            IProductoRepository repository,
            IImageStorageService imageStorage)  // NUEVO
        {
            _repository = repository;
            _imageStorage = imageStorage;
        }

        public async Task<List<ProductoBusquedaDto>> Handle(
            BuscarProductosQuery request,
            CancellationToken cancellationToken)
        {
            var resultados = await _repository.SearchWithDetalleAsync(
                request.Nombre, request.Tipo);

            return resultados.Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Tipo = p.Tipo,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Unidades = p.Unidades,
                DetalleResumen = p.DetalleResumen,
                ImagePath = _imageStorage.GetFullPath(p.ImagePath)  // NUEVO - ruta absoluta lista para la UI
            }).ToList();
        }
    }
}