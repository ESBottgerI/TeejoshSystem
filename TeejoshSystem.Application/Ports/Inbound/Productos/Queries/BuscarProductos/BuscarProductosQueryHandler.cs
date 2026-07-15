using MediatR;

using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.BuscarProductos
{
    public class BuscarProductosQueryHandler : IRequestHandler<BuscarProductosQuery, List<ProductoBusquedaDto>>
    {
        private readonly IProductoRepository _repository;
        private readonly IImageStorageService? _imageStorage;
        public BuscarProductosQueryHandler(IProductoRepository repository, IImageStorageService? imageStorage = null)
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

            var dtos = new List<ProductoBusquedaDto>();
            foreach (var p in resultados)
            {
                var thumbnail = _imageStorage is null ? null : await _imageStorage.ReadImageAsync(p.ImagePath, true, cancellationToken);
                dtos.Add(new ProductoBusquedaDto
                {
                Id = p.Id,
                Tipo = p.Tipo,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Unidades = p.Unidades,
                DetalleResumen = p.DetalleResumen,
                TieneImagen = !string.IsNullOrWhiteSpace(p.ImagePath),
                ImageThumbnail = thumbnail?.Bytes
                });
            }
            return dtos;
        }
    }
}
