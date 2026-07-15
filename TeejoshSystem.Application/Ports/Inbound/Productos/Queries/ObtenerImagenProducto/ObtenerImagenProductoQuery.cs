using MediatR;
using TeejoshSystem.Application.Common;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerImagenProducto;

public enum VarianteImagen { Thumbnail, Completa }
public sealed record ObtenerImagenProductoQuery(int ProductoId, VarianteImagen Variante) : IRequest<Result<ProductoImagenDto>>;
public sealed record ProductoImagenDto(byte[] Contenido, string ContentType);
