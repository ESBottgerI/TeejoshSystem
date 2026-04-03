using MediatR;
using TeejoshInventario.Domain.Enums;

namespace TeejoshInventario.Application.Ports.Inbound.Productos.Queries.BuscarProductos
{
    public record BuscarProductosQuery(
        string? Nombre,
        TipoProducto? Tipo
    ) : IRequest<List<ProductoBusquedaDto>>;

    public class ProductoBusquedaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Unidades { get; set; }
        public string Tipo { get; set; }
        public string DetalleResumen { get; set; }
    }
}
