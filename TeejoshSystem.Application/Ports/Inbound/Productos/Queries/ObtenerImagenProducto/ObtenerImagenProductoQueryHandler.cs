using MediatR;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerImagenProducto;

public sealed class ObtenerImagenProductoQueryHandler : IRequestHandler<ObtenerImagenProductoQuery, Result<ProductoImagenDto>>
{
    private readonly IProductoRepository _products; private readonly IImageStorageService _images;
    public ObtenerImagenProductoQueryHandler(IProductoRepository products, IImageStorageService images) { _products = products; _images = images; }
    public async Task<Result<ProductoImagenDto>> Handle(ObtenerImagenProductoQuery request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductoId);
        if (product is null || string.IsNullOrWhiteSpace(product.ImagePath)) return Result.Failure<ProductoImagenDto>("Imagen no encontrada.");
        var image = await _images.ReadImageAsync(product.ImagePath, request.Variante == VarianteImagen.Thumbnail, cancellationToken);
        return image is null ? Result.Failure<ProductoImagenDto>("Imagen no encontrada.") : Result.Success(new ProductoImagenDto(image.Bytes, image.ContentType));
    }
}
