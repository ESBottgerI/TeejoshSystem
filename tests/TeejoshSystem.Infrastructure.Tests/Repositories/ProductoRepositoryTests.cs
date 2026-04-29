using FluentAssertions;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.ValueObjects;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories;
using TeejoshSystem.Infrastructure.Tests.Fixtures;

namespace TeejoshSystem.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests de integración de ProductoRepository contra SQLite real.
/// IClassFixture comparte una sola BD por clase; LimpiarDatos() aísla cada test.
///
/// Qué se verifica aquí que los tests de Application NO pueden:
/// - Que el SQL raw de SearchAsync funciona con la columna discriminadora `type`
/// - Que ON DELETE CASCADE elimina los detalles al eliminar el producto
/// - Que las configuraciones Fluent API mapean correctamente a columnas SQLite
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

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ProductoHotWheels_DebePersistitrProductoYDetalle()
    {
        var producto = FabricarHotWheels("Ford Mustang 1968");

        await _repo.AddAsync(producto);
        await _fixture.Context.SaveChangesAsync();

        var enBd = await _repo.GetByIdAsync(producto.Id);
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
        await _fixture.Context.SaveChangesAsync();

        var enBd = await _repo.GetByIdAsync(producto.Id);
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
        await _fixture.Context.SaveChangesAsync();

        var todos = await _repo.GetAllAsync();

        todos.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_SinProductos_DebeRetornarListaVacia()
    {
        var todos = await _repo.GetAllAsync();

        todos.Should().BeEmpty();
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────
    // Estos tests son los más críticos: verifican el SQL raw con la columna `type`

    [Fact]
    public async Task SearchAsync_PorNombre_DebeRetornarSoloCoincidencias()
    {
        await _repo.AddAsync(FabricarHotWheels("Ford GT"));
        await _repo.AddAsync(FabricarHotWheels("Ford Mustang"));
        await _repo.AddAsync(FabricarHotWheels("Toyota Supra"));
        await _fixture.Context.SaveChangesAsync();

        var resultado = await _repo.SearchAsync("Ford", null);

        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(p => p.Nombre.Should().Contain("Ford"));
    }

    [Fact]
    public async Task SearchAsync_PorTipo_DebeRetornarSoloEseTipo()
    {
        await _repo.AddAsync(FabricarHotWheels("HW Test"));
        await _repo.AddAsync(FabricarFunko("Funko Test"));
        await _fixture.Context.SaveChangesAsync();

        var resultado = await _repo.SearchAsync(null, TipoProducto.HotWheels);

        resultado.Should().HaveCount(1);
        resultado.First().Tipo.Should().Be(TipoProducto.HotWheels.ToString());
    }

    [Fact]
    public async Task SearchAsync_PorNombreYTipo_DebeAplicarAmbosFiltos()
    {
        await _repo.AddAsync(FabricarHotWheels("Ford HotWheels"));
        await _repo.AddAsync(FabricarFunko("Ford Funko"));
        await _fixture.Context.SaveChangesAsync();

        var resultado = await _repo.SearchAsync("Ford", TipoProducto.HotWheels);

        resultado.Should().HaveCount(1);
        resultado.Single().Nombre.Should().Contain("Ford");
        resultado.Single().Tipo.Should().Be(TipoProducto.HotWheels.ToString());
    }

    [Fact]
    public async Task SearchAsync_SinCoincidencias_DebeRetornarVacio()
    {
        await _repo.AddAsync(FabricarHotWheels("Toyota"));
        await _fixture.Context.SaveChangesAsync();

        var resultado = await _repo.SearchAsync("BMW", null);

        resultado.Should().BeEmpty();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_DebeEliminarProductoYCascadearDetalle()
    {
        var producto = FabricarHotWheels("Para Eliminar");
        await _repo.AddAsync(producto);
        await _fixture.Context.SaveChangesAsync();
        var id = producto.Id;

        await _repo.DeleteAsync(id);
        await _fixture.Context.SaveChangesAsync();

        // El producto ya no existe
        var enBd = await _repo.GetByIdAsync(id);
        enBd.Should().BeNull();

        // El detalle tampoco existe (ON DELETE CASCADE)
        var detalleEnBd = await _fixture.Context
            .Set<HotWheelsDetalle>()
            .FindAsync(id);
        detalleEnBd.Should().BeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Producto FabricarHotWheels(string nombre)
    {
        var p = new Producto(
            TipoProducto.HotWheels,
            new NombreProducto(nombre),
            new Precio(20m),
            new Unidades(5));
        p.AsignarDescripcion(new HotWheelsDetalle
        {
            // Ajustar propiedades requeridas según tu entidad HotWheelsDetalle
            Modelo = "Modelo Test",
            Anio   = 2024
        });
        return p;
    }

    private static Producto FabricarFunko(string nombre)
    {
        var p = new Producto(
            TipoProducto.Funko,
            new NombreProducto(nombre),
            new Precio(15m),
            new Unidades(3));
        p.AsignarDescripcion(new FunkoDetalle
        {
            NumCaja  = "001",
            Licencia = "Test"
        });
        return p;
    }
}
