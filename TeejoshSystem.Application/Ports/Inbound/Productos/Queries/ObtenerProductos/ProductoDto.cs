using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Unidades { get; set; }
        public TipoProducto Tipo { get; set; }


        public string TipoDescripcion { get; set; }

        // Resumen del detalle (para mostrar en lista)
        public string DetalleResumen { get; set; }
    }
}
