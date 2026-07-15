using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductosPorId;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Entities.Detalles;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Domain.ValueObjects;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Queries;

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerProductosPorIdQueryHandler — 0% → objetivo: 33 survived, 26 no cov
//
// Mutantes objetivo:
//   - producto is null               → invertido
//   - Id, Tipo, Nombre, Precio, Unidades, ImagePath del DTO base
//   - MapearDetalleAsync: cada campo de los 5 cases
//   - HotWheels: c.Id == hw.CategoriaId  → predicado mutado
//   - HotWheels: categoria?.Nombre ?? fallback
//   - Funko:  fu.CaracteristicaEspecialId.HasValue → true/false constante
//   - TCG:    expansion is not null  → invertido
//   - TCG/HW/Funko: fallback strings cuando entidad no encontrada
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerProductosPorIdQueryHandlerTests
{
    private readonly IProductoRepository _productoRepo = Substitute.For<IProductoRepository>();
    private readonly ICatalogoRepository _catalogoRepo = Substitute.For<ICatalogoRepository>();

    private ObtenerProductosPorIdQueryHandler CrearHandler()
        => new(_productoRepo, _catalogoRepo);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Producto CrearProducto(
        TipoProducto tipo,
        int id = 1,
        string nombre = "Test",
        decimal precio = 25m,
        int stock = 3,
        string? imagePath = null)
    {
        var p = new Producto(tipo, new NombreProducto(nombre), new Precio(precio), new Unidades(stock));
        typeof(Producto).GetProperty("Id")!.SetValue(p, id);
        if (imagePath is not null) p.AsignarImagePath(imagePath);
        return p;
    }

    private static Producto ConDetalle(Producto producto, ProductoDetalle detalle)
    {
        detalle.AsignarProductoId(producto.Id > 0 ? producto.Id : 1);
        producto.AsignarDescripcion(detalle);
        return producto;
    }

    // ── Producto no encontrado ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductoNoEncontrado_RetornaFailureConIdEnMensaje()
    {
        // Mata el mutante: producto is null → producto is not null
        _productoRepo.GetByIdWithDetalleAsync(77).Returns((Producto?)null);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(77), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("77");
    }

    // ── Campos base del DTO ───────────────────────────────────────────────────

    [Fact]
    public async Task Handle_MapeaTodosLosCamposBase()
    {
        // Mata los mutantes: Id→0, Tipo→default, Nombre→null, Precio→0,
        //                    Unidades→0, ImagePath→null
        // Usamos ToyDetalle (más simple, sin catalog lookups)
        var toy = new ToyDetalle(6, 1, 4, true);
        var producto = ConDetalle(
            CrearProducto(TipoProducto.Toy, id: 55, nombre: "Monopoly", precio: 49.99m,
                          stock: 12, imagePath: "imgs/monopoly.jpg"),
            toy);

        _productoRepo.GetByIdWithDetalleAsync(55).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(55), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;
        dto.Id.Should().Be(55);
        dto.Tipo.Should().Be(TipoProducto.Toy);
        dto.Nombre.Should().Be("Monopoly");
        dto.Precio.Should().Be(49.99m);
        dto.Unidades.Should().Be(12);
        dto.TieneImagen.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DescripcionNull_DetalleEsNull()
    {
        // Producto sin descripción asignada → Detalle = null en el DTO
        var producto = CrearProducto(TipoProducto.HotWheels, id: 10);
        // No llamamos AsignarDescripcion → Descripcion = null
        _productoRepo.GetByIdWithDetalleAsync(10).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Detalle.Should().BeNull();
    }

    // ── Case HotWheels ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_HotWheels_CategoriaEncontrada_UsaNombreDeEntidad()
    {
        // Mata mutante: c.Id == hw.CategoriaId → c.Id != hw.CategoriaId
        // (con predicado invertido, FirstOrDefault devuelve categoría equivocada)
        var hw = new HotWheelsDetalle("Ferrari GTO", 2021, "Treasure Hunt", 3);
        var producto = ConDetalle(CrearProducto(TipoProducto.HotWheels, id: 1), hw);
        _productoRepo.GetByIdWithDetalleAsync(1).Returns(producto);

        // Lista con 2 categorías — solo la Id=3 es correcta
        _catalogoRepo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>
    {
        new() { Id = 1, Nombre = "Categoría Incorrecta" },
        new() { Id = 3, Nombre = "Treasure Hunt Real" }   // ← correcta
    });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(1), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<HotWheelsDetalleDto>().Subject;
        detalle.CategoriaNombre.Should().Be("Treasure Hunt Real");
    }

    [Fact]
    public async Task Handle_HotWheels_CategoriaNoEncontrada_UsaFallback()
    {
        // Mata mutante: categoria?.Nombre → categoria.Nombre (sin null check)
        // y el fallback string
        var hw = new HotWheelsDetalle("Supra", 2020, "Basic", 99);
        var producto = ConDetalle(CrearProducto(TipoProducto.HotWheels, id: 2), hw);
        _productoRepo.GetByIdWithDetalleAsync(2).Returns(producto);

        // Lista vacía → categoria = null → fallback
        _catalogoRepo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>());

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(2), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<HotWheelsDetalleDto>().Subject;
        detalle.CategoriaNombre.Should().Be("Categoría 99");
    }

    [Fact]
    public async Task Handle_HotWheels_MapeaTodosLosCampos()
    {
        // Mata mutantes de Modelo, Anio, Serie, CategoriaId en HotWheelsDetalleDto
        var hw = new HotWheelsDetalle("Ferrari 250 GTO", 2019, "TH Real", 5);
        var producto = ConDetalle(CrearProducto(TipoProducto.HotWheels, id: 3), hw);
        _productoRepo.GetByIdWithDetalleAsync(3).Returns(producto);

        _catalogoRepo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>
    {
        new() { Id = 5, Nombre = "Treasure Hunt" }
    });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(3), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<HotWheelsDetalleDto>().Subject;
        detalle.Modelo.Should().Be("Ferrari 250 GTO");
        detalle.Anio.Should().Be(2019);
        detalle.Serie.Should().Be("TH Real");
        detalle.CategoriaId.Should().Be(5);
        detalle.Tipo.Should().Be(TipoProducto.HotWheels);
    }

    // ── Case Funko ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Funko_SinCaracteristicaEspecial_NoLlamaGetCaracteristicas()
    {
        // Mata el mutante: fu.CaracteristicaEspecialId.HasValue → true (siempre llama)
        var funko = new FunkoDetalle(1138, "Batman DC", 2, null); // sin CaractEspecial
        var producto = ConDetalle(CrearProducto(TipoProducto.Funko, id: 4), funko);
        _productoRepo.GetByIdWithDetalleAsync(4).Returns(producto);
        _catalogoRepo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());

        await CrearHandler().Handle(new ObtenerProductosPorIdQuery(4), CancellationToken.None);

        await _catalogoRepo.DidNotReceive().GetFunkoCaracteristicasAsync();
    }

    [Fact]
    public async Task Handle_Funko_ConCaracteristicaEspecial_LlamaGetCaracteristicasYMapea()
    {
        // Mata el mutante: fu.CaracteristicaEspecialId.HasValue → false (nunca llama)
        var funko = new FunkoDetalle(500, "Naruto", 1, 7); // con CaractEspecial = 7
        var producto = ConDetalle(CrearProducto(TipoProducto.Funko, id: 5), funko);
        _productoRepo.GetByIdWithDetalleAsync(5).Returns(producto);
        _catalogoRepo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());
        _catalogoRepo.GetFunkoCaracteristicasAsync().Returns(new List<FunkoCaracteristica>
    {
        new() { Id = 7, Nombre = "Glow in the Dark" }
    });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(5), CancellationToken.None);

        await _catalogoRepo.Received(1).GetFunkoCaracteristicasAsync();
        var detalle = result.Value.Detalle.Should().BeOfType<FunkoDetalleDto>().Subject;
        detalle.CaracteristicaEspecialNombre.Should().Be("Glow in the Dark");
    }

    [Fact]
    public async Task Handle_Funko_MapeaTodosLosCampos()
    {
        // Mata mutantes: NumeroBox, Licencia, SubtipoId, SubtipoNombre, CaractEspecialId
        var funko = new FunkoDetalle(1138, "Batman DC", 3, null);
        var producto = ConDetalle(CrearProducto(TipoProducto.Funko, id: 6), funko);
        _productoRepo.GetByIdWithDetalleAsync(6).Returns(producto);
        _catalogoRepo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>
    {
        new() { Id = 3, Nombre = "Pop!" }
    });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(6), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<FunkoDetalleDto>().Subject;
        detalle.NumeroBox.Should().Be(1138);
        detalle.Licencia.Should().Be("Batman DC");
        detalle.SubtipoId.Should().Be(3);
        detalle.SubtipoNombre.Should().Be("Pop!");
        detalle.CaracteristicaEspecialId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Funko_SubtipoNoEncontrado_UsaFallback()
    {
        // Mata mutante: subtipo?.Nombre ?? $"Subtipo {fu.SubtipoId}"
        var funko = new FunkoDetalle(200, "Marvel", 99, null);
        var producto = ConDetalle(CrearProducto(TipoProducto.Funko, id: 7), funko);
        _productoRepo.GetByIdWithDetalleAsync(7).Returns(producto);
        _catalogoRepo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(7), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<FunkoDetalleDto>().Subject;
        detalle.SubtipoNombre.Should().Be("Subtipo 99");
    }

    // ── Case TCG ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Tcg_ExpansionEncontrada_FranquiciaEncontrada()
    {
        // Mata mutante: expansion is not null → false (siempre null → "Sin franquicia")
        var tcg = new TcgDetalle(10, 20);
        var producto = ConDetalle(CrearProducto(TipoProducto.Tcg, id: 8), tcg);
        _productoRepo.GetByIdWithDetalleAsync(8).Returns(producto);

        _catalogoRepo.GetTcgExpansionByIdAsync(20)
                     .Returns(new TcgExpansion { Id = 20, Nombre = "Base Set", FranquiciaId = 2 });
        _catalogoRepo.GetTcgPackByIdAsync(10)
                     .Returns(new TcgPack { Id = 10, Nombre = "Booster Box" });
        _catalogoRepo.GetTcgFranquiciasAsync()
                     .Returns(new List<TcgFranquicia>
                     {
                     new() { Id = 1, Nombre = "Magic" },
                     new() { Id = 2, Nombre = "Pokémon" }  // ← FranquiciaId = 2
                     });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(8), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<TcgDetalleDto>().Subject;
        detalle.ExpansionNombre.Should().Be("Base Set");
        detalle.PackNombre.Should().Be("Booster Box");
        detalle.FranquiciaNombre.Should().Be("Pokémon");
    }

    [Fact]
    public async Task Handle_Tcg_ExpansionNull_UsaFallbacksYFranquiciaNula()
    {
        // Mata mutante: expansion is not null → true (siempre busca franquicia)
        var tcg = new TcgDetalle(5, 88);
        var producto = ConDetalle(CrearProducto(TipoProducto.Tcg, id: 9), tcg);
        _productoRepo.GetByIdWithDetalleAsync(9).Returns(producto);

        _catalogoRepo.GetTcgExpansionByIdAsync(88).Returns((TcgExpansion?)null);
        _catalogoRepo.GetTcgPackByIdAsync(5).Returns((TcgPack?)null);
        _catalogoRepo.GetTcgFranquiciasAsync().Returns(new List<TcgFranquicia>());

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(9), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<TcgDetalleDto>().Subject;
        detalle.ExpansionNombre.Should().Be("Expansión 88");
        detalle.PackNombre.Should().Be("Pack 5");
        detalle.FranquiciaNombre.Should().Be("Sin franquicia");
    }

    [Fact]
    public async Task Handle_Tcg_MapeaTodosLosCampos()
    {
        // Mata mutantes: PackId, PackNombre, ExpansionId, ExpansionNombre, FranquiciaNombre
        var tcg = new TcgDetalle(3, 7);
        var producto = ConDetalle(CrearProducto(TipoProducto.Tcg, id: 10), tcg);
        _productoRepo.GetByIdWithDetalleAsync(10).Returns(producto);

        _catalogoRepo.GetTcgExpansionByIdAsync(7)
                     .Returns(new TcgExpansion { Id = 7, Nombre = "Neo Genesis", FranquiciaId = 1 });
        _catalogoRepo.GetTcgPackByIdAsync(3)
                     .Returns(new TcgPack { Id = 3, Nombre = "Starter Deck" });
        _catalogoRepo.GetTcgFranquiciasAsync()
                     .Returns(new List<TcgFranquicia> { new() { Id = 1, Nombre = "Pokémon" } });

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(10), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<TcgDetalleDto>().Subject;
        detalle.PackId.Should().Be(3);
        detalle.PackNombre.Should().Be("Starter Deck");
        detalle.ExpansionId.Should().Be(7);
        detalle.ExpansionNombre.Should().Be("Neo Genesis");
        detalle.FranquiciaNombre.Should().Be("Pokémon");
        detalle.Tipo.Should().Be(TipoProducto.Tcg);
    }

    // ── Case Toy ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Toy_MapeaTodosLosCampos()
    {
        // Mata mutantes: EdadMinima, JugadoresMinimo, JugadoresMaximo, EsJuegoMesa
        var toy = new ToyDetalle(8, 2, 6, true);
        var producto = ConDetalle(CrearProducto(TipoProducto.Toy, id: 11), toy);
        _productoRepo.GetByIdWithDetalleAsync(11).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(11), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<ToyDetalleDto>().Subject;
        detalle.EdadMinima.Should().Be(8);
        detalle.JugadoresMinimo.Should().Be(2);
        detalle.JugadoresMaximo.Should().Be(6);
        detalle.EsJuegoMesa.Should().BeTrue();
        detalle.Tipo.Should().Be(TipoProducto.Toy);
    }

    [Fact]
    public async Task Handle_Toy_EsJuegoDeMesaFalse_MapeaCorrectamente()
    {
        // Mata mutante: EsJuegoDeMesa → !EsJuegoDeMesa
        var toy = new ToyDetalle(3, 1, 1, false);
        var producto = ConDetalle(CrearProducto(TipoProducto.Toy, id: 12), toy);
        _productoRepo.GetByIdWithDetalleAsync(12).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(12), CancellationToken.None);

        result.Value.Detalle.Should().BeOfType<ToyDetalleDto>()
              .Which.EsJuegoMesa.Should().BeFalse();
    }

    // ── Case Varios ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_Varios_MapeaTodosLosCampos()
    {
        // Mata mutantes: Marca, Alto, Ancho, Largo, Material, TieneIlustracion
        var varios = new VariosDetalle("Bandai", 15m, 10m, 5m, "Plástico", true);
        var producto = ConDetalle(CrearProducto(TipoProducto.Varios, id: 13), varios);
        _productoRepo.GetByIdWithDetalleAsync(13).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(13), CancellationToken.None);

        var detalle = result.Value.Detalle.Should().BeOfType<VariosDetalleDto>().Subject;
        detalle.Marca.Should().Be("Bandai");
        detalle.Alto.Should().Be(15m);
        detalle.Ancho.Should().Be(10m);
        detalle.Largo.Should().Be(5m);
        detalle.Material.Should().Be("Plástico");
        detalle.TieneIlustracion.Should().BeTrue();
        detalle.Tipo.Should().Be(TipoProducto.Varios);
    }

    [Fact]
    public async Task Handle_Varios_LargoNull_MapeaCorrectamente()
    {
        // Mata mutante: va.Largo → 0 cuando es null
        var varios = new VariosDetalle("Funko", 10m, 8m, null, "PVC", false);
        var producto = ConDetalle(CrearProducto(TipoProducto.Varios, id: 14), varios);
        _productoRepo.GetByIdWithDetalleAsync(14).Returns(producto);

        var result = await CrearHandler().Handle(
            new ObtenerProductosPorIdQuery(14), CancellationToken.None);

        result.Value.Detalle.Should().BeOfType<VariosDetalleDto>()
              .Which.Largo.Should().BeNull();
    }
}
