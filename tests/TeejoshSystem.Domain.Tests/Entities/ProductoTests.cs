using FluentAssertions;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Tests.Entities;

/// <summary>
/// Cubre las invariantes de la entidad raíz Producto.
/// No se testean getters triviales ni el constructor de EF (privado).
/// </summary>
public class ProductoTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Producto CrearProducto(TipoProducto tipo, string nombre = "Test", decimal precio = 10m, int stock = 5)
        => new(tipo,
               new NombreProducto(nombre),
               new Precio(precio),
               new Unidades(stock));

    // ── AsignarDescripcion ────────────────────────────────────────────────────

    [Fact]
    public void AsignarDescripcion_HotWheelsEnProductoHotWheels_DebeAsignar()
    {
        var producto = CrearProducto(TipoProducto.HotWheels);
        var detalle = new HotWheelsDetalle(); // ajustar propiedades si el constructor lo requiere

        var act = () => producto.AsignarDescripcion(detalle);

        act.Should().NotThrow();
        producto.Descripcion.Should().BeSameAs(detalle);
    }

    [Fact]
    public void AsignarDescripcion_FunkoEnProductoHotWheels_DebeArrojarInvalidOperationException()
    {
        var producto = CrearProducto(TipoProducto.HotWheels);
        var detalleIncorrecto = new FunkoDetalle();

        var act = () => producto.AsignarDescripcion(detalleIncorrecto);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*FunkoDetalle*")
           .And.Message.Should().Contain("HotWheels");
    }

    [Theory]
    [MemberData(nameof(DetalleTipoIncorrectoCasos))]
    public void AsignarDescripcion_ConDetalleTipoIncorrecto_SiempreArrojaExcepcion(
        TipoProducto tipoProducto, ProductoDetalle detalleIncorrecto)
    {
        var producto = CrearProducto(tipoProducto);

        var act = () => producto.AsignarDescripcion(detalleIncorrecto);

        act.Should().Throw<InvalidOperationException>();
    }

    public static IEnumerable<object[]> DetalleTipoIncorrectoCasos()
    {
        yield return [TipoProducto.HotWheels, new FunkoDetalle()];
        yield return [TipoProducto.HotWheels, new TcgDetalle()];
        yield return [TipoProducto.Funko,     new HotWheelsDetalle()];
        yield return [TipoProducto.Tcg,       new ToyDetalle()];
        yield return [TipoProducto.Toy,       new VariosDetalle()];
        yield return [TipoProducto.Varios,    new HotWheelsDetalle()];
    }

    [Fact]
    public void AsignarDescripcion_ConNull_DebeArrojarArgumentNullException()
    {
        var producto = CrearProducto(TipoProducto.Funko);

        var act = () => producto.AsignarDescripcion(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── ReducirStock ──────────────────────────────────────────────────────────

    [Fact]
    public void ReducirStock_ConStockSuficiente_DebeActualizarStock()
    {
        var producto = CrearProducto(TipoProducto.HotWheels, stock: 10);

        producto.ReducirStock(3);

        producto.Stock.Value.Should().Be(7);
    }

    [Fact]
    public void ReducirStock_CantidadExacta_DebeDejarStockEnCero()
    {
        var producto = CrearProducto(TipoProducto.HotWheels, stock: 5);

        producto.ReducirStock(5);

        producto.Stock.Value.Should().Be(0);
    }

    [Fact]
    public void ReducirStock_SinStockSuficiente_DebeArrojarInvalidOperationException()
    {
        // Invariante crítica: nunca stock negativo
        var producto = CrearProducto(TipoProducto.HotWheels, stock: 2);

        var act = () => producto.ReducirStock(5);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public void ReducirStock_StockEnCero_DebeArrojar()
    {
        var producto = CrearProducto(TipoProducto.HotWheels, stock: 0);

        var act = () => producto.ReducirStock(1);

        act.Should().Throw<InvalidOperationException>();
    }

    // ── AgregarStock ──────────────────────────────────────────────────────────

    [Fact]
    public void AgregarStock_DebeIncrementarStock()
    {
        var producto = CrearProducto(TipoProducto.Funko, stock: 3);

        producto.AgregarStock(7);

        producto.Stock.Value.Should().Be(10);
    }

    // ── ActualizarDatos ───────────────────────────────────────────────────────

    [Fact]
    public void ActualizarDatos_ConDatosValidos_DebeReemplazarPropiedades()
    {
        var producto = CrearProducto(TipoProducto.HotWheels, nombre: "Original", precio: 10m, stock: 1);

        producto.ActualizarDatos(
            new NombreProducto("Actualizado"),
            new Precio(99.99m),
            new Unidades(50));

        producto.Nombre.Value.Should().Be("Actualizado");
        producto.Precio.Value.Should().Be(99.99m);
        producto.Stock.Value.Should().Be(50);
    }

    [Fact]
    public void ActualizarDatos_ConNombreNull_DebeArrojarArgumentNullException()
    {
        var producto = CrearProducto(TipoProducto.Funko);

        var act = () => producto.ActualizarDatos(null!, new Precio(10m), new Unidades(1));

        act.Should().Throw<ArgumentNullException>();
    }

    // ── CambiarPrecio ─────────────────────────────────────────────────────────

    [Fact]
    public void CambiarPrecio_DebeReemplazarPrecio()
    {
        var producto = CrearProducto(TipoProducto.Toy, precio: 5m);

        producto.CambiarPrecio(new Precio(25m));

        producto.Precio.Value.Should().Be(25m);
    }
}
