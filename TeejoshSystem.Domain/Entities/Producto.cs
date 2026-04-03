using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Entities
{
    public class Producto
    {
        public int Id { get; private set; }
        public NombreProducto Nombre { get; private set; }
        public Precio Precio { get; private set; }
        public Unidades Stock { get; private set; }
        public ProductoDetalle Descripcion { get; private set; }

        private Producto() { } // Para EF (Infrastructure)

        public Producto(
            NombreProducto nombre,
            Precio precio,
            Unidades stock)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Precio = precio ?? throw new ArgumentNullException(nameof(precio));
            Stock = stock ?? throw new ArgumentNullException(nameof(stock));
        }

        public void ActualizarDatos(
            NombreProducto nombre,
            Precio precio,
            Unidades stock)
        {
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Precio = precio ?? throw new ArgumentNullException(nameof(precio));
            Stock = stock ?? throw new ArgumentNullException(nameof(stock));
        }

        // Precio
        public void CambiarPrecio(Precio nuevoPrecio)
        {
            Precio = nuevoPrecio;
        }

        // Stock
        public void AgregarStock(int cantidad)
        {
            Stock = Stock.Incrementar(cantidad);
        }

        public void ReducirStock(int cantidad)
        {
            Stock = Stock.Decrementar(cantidad);
        }

        // Descripcion
        public void AsignarDescripcion(ProductoDetalle descripcion)
        {
            ArgumentNullException.ThrowIfNull(descripcion);

            Descripcion = descripcion;
        }
    }
}
