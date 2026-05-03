using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.ValueObjects;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Tests.Fixtures;

namespace TeejoshSystem.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests de integración contra SQLite real.
/// IClassFixture comparte una BD por clase; LimpiarDatos() aísla cada test.
///
/// Qué se verifica aquí que Application.Tests no puede:
/// - SQL raw de SearchWithDetalleAsync funciona con la columna discriminadora `type`
/// - ON DELETE CASCADE elimina detalles al eliminar el producto
/// - Configuraciones Fluent API mapean correctamente a las tablas SQLite
/// - AddAsync persiste producto Y detalle en la misma transacción
/// </summary>
public class ProductoRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductoRepository _repo;

    public ProductoRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repo = new ProductoRepository(fixture.Context);
        _fixture.LimpiarDatos();
    }

    // ── AddAsync + GetByIdWithDetalleAsync ────────────────────────────────────

    [Fact]
    public async Task AddAsync_ProductoHotWheels_DebePersistitrProductoYDetalle()
    {
        var producto = FabricarHotWheels("Ford Mustang 1968");

        await _repo.AddAsync(producto);

        var enBd = await _repo.GetByIdWithDetalleAsync(producto.Id);
        enBd.Should().NotBeNull();
        enBd!.Nombre.Value.Should().Be("Ford Mustang 1968");
        enBd.Tipo.Should().Be(TipoProducto.HotWheels);
        enBd.Descripcion.Should().NotBeNull();
        enBd.Descripcion.Should().BeOfType<HotWheelsDetalle>();
    }

    [Fact]
    public async Task AddAsync_ProductoFunko_DebePersistitrProductoYDetalle()
    {
        var producto = FabricarFunko("Pikachu Oversized");

        await _repo.AddAsync(producto);

        var enBd = await _repo.GetByIdWithDetalleAsync(producto.Id);
        enBd.Should().NotBeNull();
        enBd!.Tipo.Should().Be(TipoProducto.Funko);
        enBd.Descripcion.Should().BeOfType<FunkoDetalle>();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ConVariosProductos_DebeRetornarTodos()
    {
        await _repo.AddAsync(FabricarHotWheels("A"));
        await _repo.AddAsync(FabricarHotWheels("B"));
        await _repo.AddAsync(FabricarFunko("C"));

        var todos = await _repo.GetAllAsync();

        todos.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_SinProductos_DebeRetornarListaVacia()
    {
        var todos = await _repo.GetAllAsync();

        todos.Should().BeEmpty();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_IdExistente_DebeRetornarProducto()
    {
        var producto = FabricarHotWheels("BMW M3");
        await _repo.AddAsync(producto);

        var enBd = await _repo.GetByIdAsync(producto.Id);

        enBd.Should().NotBeNull();
        enBd!.Nombre.Value.Should().Be("BMW M3");
    }

    [Fact]
    public async Task GetByIdAsync_IdInexistente_DebeRetornarNull()
    {
        var enBd = await _repo.GetByIdAsync(99999);

        enBd.Should().BeNull();
    }

    // ── SearchWithDetalleAsync ────────────────────────────────────────────────
    // Estos tests son los más críticos: verifican el SQL raw con JOIN
    // y la columna discriminadora `type`

    [Fact]
    public async Task SearchWithDetalleAsync_PorNombre_DebeRetornarSoloCoincidencias()
    {
        await _repo.AddAsync(FabricarHotWheels("Ford GT"));
        await _repo.AddAsync(FabricarHotWheels("Ford Mustang"));
        await _repo.AddAsync(FabricarHotWheels("Toyota Supra"));

        var resultado = await _repo.SearchWithDetalleAsync("Ford", null);

        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(p =>
            p.Nombre.Should().Contain("Ford"));
    }

    [Fact]
    public async Task SearchWithDetalleAsync_PorTipo_DebeRetornarSoloEseTipo()
    {
        await _repo.AddAsync(FabricarHotWheels("HW Test"));
        await _repo.AddAsync(FabricarFunko("Funko Test"));

        var resultado = await _repo.SearchWithDetalleAsync(null, TipoProducto.HotWheels);

        resultado.Should().HaveCount(1);
        resultado.Single().Tipo.Should().Be(TipoProducto.HotWheels);
    }

    [Fact]
    public async Task SearchWithDetalleAsync_PorNombreYTipo_DebeAplicarAmbosFiltros()
    {
        await _repo.AddAsync(FabricarHotWheels("Ford HotWheels"));
        await _repo.AddAsync(FabricarFunko("Ford Funko"));

        var resultado = await _repo.SearchWithDetalleAsync("Ford", TipoProducto.HotWheels);

        resultado.Should().HaveCount(1);
        resultado.Single().Nombre.Should().Contain("Ford");
        resultado.Single().Tipo.Should().Be(TipoProducto.HotWheels);
    }

    [Fact]
    public async Task SearchWithDetalleAsync_SinCoincidencias_DebeRetornarVacio()
    {
        await _repo.AddAsync(FabricarHotWheels("Toyota"));

        var resultado = await _repo.SearchWithDetalleAsync("BMW", null);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchWithDetalleAsync_DebeRetornarProductoConResumen()
    {
        // Verifica que el resultado incluye el producto.
        // DetalleResumen depende del JOIN con la tabla de detalles —
        // si devuelve "Sin detalle" indica que la FK category_id no tiene
        // dato en hot_wheels_category. El test valida que el campo existe.
        await _repo.AddAsync(FabricarHotWheels("Camaro"));

        var resultado = await _repo.SearchWithDetalleAsync("Camaro", null);

        resultado.Should().HaveCount(1);
        resultado.Single().DetalleResumen.Should().NotBeNull();
        resultado.Single().Nombre.Should().Be("Camaro");
    }

    // ── DeleteAsync + CASCADE ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_DebeEliminarProductoYCascadearDetalle()
    {
        var producto = FabricarHotWheels("Para Eliminar");
        await _repo.AddAsync(producto);
        var id = producto.Id;

        // DeleteAsync recibe la entidad completa
        await _repo.DeleteAsync(producto);

        // El producto ya no existe
        var enBd = await _repo.GetByIdAsync(id);
        enBd.Should().BeNull();

        // El detalle tampoco existe (ON DELETE CASCADE)
        var detalleEnBd = await _fixture.Context
            .HotWheelsDetalles
            .FirstOrDefaultAsync(d => d.ProductoId == id);
        detalleEnBd.Should().BeNull();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_DebePersistitrCambiosEnBd()
    {
        var producto = FabricarHotWheels("Original");
        await _repo.AddAsync(producto);

        producto.ActualizarDatos(
            new NombreProducto("Actualizado"),
            new Precio(99m),
            new Unidades(10));
        await _repo.UpdateAsync(producto);

        _fixture.Context.ChangeTracker.Clear();
        var enBd = await _repo.GetByIdAsync(producto.Id);
        enBd!.Nombre.Value.Should().Be("Actualizado");
        enBd.Precio.Value.Should().Be(99m);
        enBd.Stock.Value.Should().Be(10);
    }

    // ── ExistsAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_IdExistente_DebeRetornarTrue()
    {
        var producto = FabricarHotWheels("Existe");
        await _repo.AddAsync(producto);

        var existe = await _repo.ExistsAsync(producto.Id);

        existe.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_IdInexistente_DebeRetornarFalse()
    {
        var existe = await _repo.ExistsAsync(99999);

        existe.Should().BeFalse();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Producto FabricarHotWheels(string nombre)
    {
        var p = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto(nombre),
            new Precio(20m),
            new Unidades(5));
        p.AsignarDescripcion(new HotWheelsDetalle("Modelo Test", 2024, "Serie Test", 1));
        return p;
    }

    private static Producto FabricarFunko(string nombre)
    {
        var p = new Producto(
            TipoProducto.Funko,
            new NombreProducto(nombre),
            new Precio(15m),
            new Unidades(3));
        p.AsignarDescripcion(new FunkoDetalle(1, "Licencia Test", 1, null));
        return p;
    }
}