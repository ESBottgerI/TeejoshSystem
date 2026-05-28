using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Domain.Tests.Entities;
 
/// <summary>
/// Cubre las invariantes de la entidad raíz Producto.
///
/// NOTA sobre constructores de detalles:
/// Todos tienen constructor privado sin parámetros (para EF) y constructor
/// público con parámetros requeridos. No se pueden instanciar con new().
/// Los helpers de este archivo usan valores mínimos válidos según las
/// validaciones de cada constructor.
/// </summary>
public class ProductoTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────
 
    private static Producto CrearProducto(
        TipoProducto tipo,
        string nombre = "Test",
        decimal precio = 10m,
        int stock = 5)
        => new(tipo,
               new NombreProducto(nombre),
               new Precio(precio),
               new Unidades(stock));
 
    // HotWheels: (modelo, anio, serie, categoriaId)
    private static HotWheelsDetalle HotWheels()
        => new("Modelo Test", 2020, "Serie Test", 1);
 
    // Funko: (numeroCaja, licencia, subtipoId, caracteristicaEspecialId)
    private static FunkoDetalle Funko()
        => new(1, "Licencia Test", 1, null);
 
    // Tcg: (packId, expansionId)
    private static TcgDetalle Tcg()
        => new(1, 1);
 
    // Toy: (edadMinima, jugadoresMin, jugadoresMax, esJuegoDeMesa)
    private static ToyDetalle Toy()
        => new(3, 2, 4, false);
 
    // Varios: (marca, alto, ancho, largo, material, tieneIlustracion)
    private static VariosDetalle Varios()
        => new("Marca Test", 10m, 5m, null, "Plástico", false);
 
    // ── AsignarDescripcion — tipo correcto ────────────────────────────────────
 
    [Fact]
    public void AsignarDescripcion_HotWheelsEnProductoHotWheels_DebeAsignar()
    {
        var producto = CrearProducto(TipoProducto.HotWheels);
        var detalle  = HotWheels();
 
        var act = () => producto.AsignarDescripcion(detalle);
 
        act.Should().NotThrow();
        producto.Descripcion.Should().BeSameAs(detalle);
    }
 
    [Fact]
    public void AsignarDescripcion_FunkoEnProductoFunko_DebeAsignar()
    {
        var producto = CrearProducto(TipoProducto.Funko);
        var detalle  = Funko();
 
        var act = () => producto.AsignarDescripcion(detalle);
 
        act.Should().NotThrow();
        producto.Descripcion.Should().BeSameAs(detalle);
    }
 
    // ── AsignarDescripcion — tipo incorrecto ──────────────────────────────────
 
    [Fact]
    public void AsignarDescripcion_FunkoEnProductoHotWheels_DebeArrojarConMensajeDescriptivo()
    {
        var producto = CrearProducto(TipoProducto.HotWheels);
 
        var act = () => producto.AsignarDescripcion(Funko());
 
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*FunkoDetalle*");
    }
 
    [Theory]
    [MemberData(nameof(DetalleTipoIncorrectoCasos))]
    public void AsignarDescripcion_ConDetalleTipoIncorrecto_SiempreArrojaInvalidOperationException(
        TipoProducto tipoProducto,
        ProductoDetalle detalleIncorrecto)
    {
        var producto = CrearProducto(tipoProducto);
 
        var act = () => producto.AsignarDescripcion(detalleIncorrecto);
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    /// <summary>
    /// Cada fila: (tipo del producto, detalle incompatible con ese tipo).
    /// Se usan dos detalles distintos por tipo para verificar que la
    /// validación no es accidental.
    /// </summary>
    public static IEnumerable<object[]> DetalleTipoIncorrectoCasos()
    {
        yield return [TipoProducto.HotWheels, Funko()];
        yield return [TipoProducto.HotWheels, Tcg()];
        yield return [TipoProducto.Funko,     HotWheels()];
        yield return [TipoProducto.Funko,     Toy()];
        yield return [TipoProducto.Tcg,       Funko()];
        yield return [TipoProducto.Toy,       Varios()];
        yield return [TipoProducto.Varios,    HotWheels()];
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
    public void ReducirStock_CantidadMayorAlStock_DebeArrojarInvalidOperationException()
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

    // ── TempBug ─────────────────────────────────────────────────────────

    // ── Constructor: Tipo = tipo nunca afirmado directamente ──────────────────

    [Theory]
    [InlineData(TipoProducto.HotWheels)]
    [InlineData(TipoProducto.Funko)]
    [InlineData(TipoProducto.Tcg)]
    [InlineData(TipoProducto.Toy)]
    [InlineData(TipoProducto.Varios)]
    public void Constructor_TipoAsignadoCorrectamente(TipoProducto tipo)
    {
        // Mata el mutante que reemplaza Tipo = tipo → Tipo = default
        var producto = CrearProducto(tipo);

        producto.Tipo.Should().Be(tipo);
    }

    // ── Constructor: null guards (nombre, precio, stock) ─────────────────────

    [Fact]
    public void Constructor_NombreNull_DebeArrojarArgumentNullException()
    {
        // Mata el mutante que elimina: nombre ?? throw
        var act = () => new Producto(
            TipoProducto.HotWheels,
            null!,
            new Precio(10m),
            new Unidades(5));

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("nombre");
    }

    [Fact]
    public void Constructor_PrecioNull_DebeArrojarArgumentNullException()
    {
        // Mata el mutante que elimina: precio ?? throw
        var act = () => new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Test"),
            null!,
            new Unidades(5));

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("precio");
    }

    [Fact]
    public void Constructor_StockNull_DebeArrojarArgumentNullException()
    {
        // Mata el mutante que elimina: stock ?? throw
        var act = () => new Producto(
            TipoProducto.HotWheels,
            new NombreProducto("Test"),
            new Precio(10m),
            null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("stock");
    }

    // ── ActualizarDatos: null guards de precio y stock ────────────────────────

    [Fact]
    public void ActualizarDatos_PrecioNull_DebeArrojarArgumentNullException()
    {
        // Mata el mutante que elimina: precio ?? throw en ActualizarDatos
        var producto = CrearProducto(TipoProducto.HotWheels);

        var act = () => producto.ActualizarDatos(
            new NombreProducto("Nuevo"),
            null!,
            new Unidades(10));

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("precio");
    }

    [Fact]
    public void ActualizarDatos_StockNull_DebeArrojarArgumentNullException()
    {
        // Mata el mutante que elimina: stock ?? throw en ActualizarDatos
        var producto = CrearProducto(TipoProducto.HotWheels);

        var act = () => producto.ActualizarDatos(
            new NombreProducto("Nuevo"),
            new Precio(20m),
            null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("stock");
    }

    // ── AsignarImagePath: 2 mutantes sin cobertura ───────────────────────────

    [Fact]
    public void AsignarImagePath_ConRuta_AsignaImagePath()
    {
        // Mata el mutante ImagePath = imagePath → ImagePath = null
        var producto = CrearProducto(TipoProducto.HotWheels);

        producto.AsignarImagePath("imagenes/pikachu.jpg");

        producto.ImagePath.Should().Be("imagenes/pikachu.jpg");
    }

    [Fact]
    public void AsignarImagePath_ConNull_DejaImagePathNull()
    {
        // Mata el mutante que elimina la asignación cuando imagePath es null
        var producto = CrearProducto(TipoProducto.HotWheels);
        producto.AsignarImagePath("ruta_anterior.jpg"); // primero asignar algo

        producto.AsignarImagePath(null);

        producto.ImagePath.Should().BeNull();
    }
}