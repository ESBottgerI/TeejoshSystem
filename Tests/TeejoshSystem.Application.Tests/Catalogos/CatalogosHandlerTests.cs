using TeejoshSystem.Application.Ports.Inbound.Catalogos.Commands.SincronizarCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerImagenExpansion;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Catalogos;

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerImagenExpansionQueryHandler — 0% (2 survived, 2 no cov)
//
// Mutantes objetivo:
//   - expansion?.ImageUrl → expansion.ImageUrl (null propagation)
//   - return null cuando expansion es null
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerImagenExpansionQueryHandlerTests
{
    private readonly ICatalogoRepository _repo = Substitute.For<ICatalogoRepository>();

    private ObtenerImagenExpansionQueryHandler CrearHandler()
        => new(_repo);

    [Fact]
    public async Task Handle_ExpansionConImageUrl_RetornaImageUrl()
    {
        // Mata el mutante: expansion?.ImageUrl → null constante
        _repo.GetTcgExpansionByIdAsync(5)
             .Returns(new TcgExpansion { Id = 5, Nombre = "Base Set", ImageUrl = "imgs/base_set.png" });

        var result = await CrearHandler().Handle(
            new ObtenerImagenExpansionQuery(5), CancellationToken.None);

        result.Should().Be("imgs/base_set.png");
    }

    [Fact]
    public async Task Handle_ExpansionSinImageUrl_RetornaNull()
    {
        // Mata el mutante: expansion?.ImageUrl → string.Empty
        _repo.GetTcgExpansionByIdAsync(6)
             .Returns(new TcgExpansion { Id = 6, Nombre = "Jungle", ImageUrl = null });

        var result = await CrearHandler().Handle(
            new ObtenerImagenExpansionQuery(6), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ExpansionNoEncontrada_RetornaNull()
    {
        // Mata el mutante: expansion?.ImageUrl → acceso directo sin null check
        _repo.GetTcgExpansionByIdAsync(99).Returns((TcgExpansion?)null);

        var result = await CrearHandler().Handle(
            new ObtenerImagenExpansionQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvocaGetTcgExpansionByIdAsyncConIdCorrecto()
    {
        // Mata mutante: request.ExpansionId → 0
        _repo.GetTcgExpansionByIdAsync(Arg.Any<int>()).Returns((TcgExpansion?)null);

        await CrearHandler().Handle(new ObtenerImagenExpansionQuery(33), CancellationToken.None);

        await _repo.Received(1).GetTcgExpansionByIdAsync(33);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerCatalogosQueryHandler — 0% (7 survived, 2 no cov)
//
// Mutantes objetivo — 4 colecciones del DTO:
//   - Id y Nombre en cada Select de las 4 colecciones
//   - CategoriasHotWheels asignado vs no asignado
//   - SubtiposFunko, CaracteristicasFunko, FranquiciasTcg ídem
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerCatalogosQueryHandlerTests
{
    private readonly ICatalogoRepository _repo = Substitute.For<ICatalogoRepository>();

    private ObtenerCatalogosQueryHandler CrearHandler() => new(_repo);

    [Fact]
    public async Task Handle_MapeoCategoriasHotWheels_IdYNombreCorrectos()
    {
        // Mata mutantes: c.Id → 0, c.Nombre → null en la projection de HotWheels
        _repo.GetHotWheelsCategoriasAsync()
             .Returns(new List<HotWheelsCategoria> { new() { Id = 7, Nombre = "Treasure Hunt" } });
        _repo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());
        _repo.GetFunkoCaracteristicasAsync().Returns(new List<FunkoCaracteristica>());
        _repo.GetTcgFranquiciasAsync().Returns(new List<TcgFranquicia>());

        var result = await CrearHandler().Handle(new ObtenerCatalogosQuery(), CancellationToken.None);

        result.CategoriasHotWheels.Should().HaveCount(1);
        result.CategoriasHotWheels[0].Id.Should().Be(7);
        result.CategoriasHotWheels[0].Nombre.Should().Be("Treasure Hunt");
    }

    [Fact]
    public async Task Handle_MapeoSubtiposFunko_IdYNombreCorrectos()
    {
        // Mata mutantes: s.Id → 0, s.Nombre → null en SubtiposFunko
        _repo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>());
        _repo.GetFunkoSubtiposAsync()
             .Returns(new List<FunkoSubtipo> { new() { Id = 3, Nombre = "Pop!" } });
        _repo.GetFunkoCaracteristicasAsync().Returns(new List<FunkoCaracteristica>());
        _repo.GetTcgFranquiciasAsync().Returns(new List<TcgFranquicia>());

        var result = await CrearHandler().Handle(new ObtenerCatalogosQuery(), CancellationToken.None);

        result.SubtiposFunko.Should().HaveCount(1);
        result.SubtiposFunko[0].Id.Should().Be(3);
        result.SubtiposFunko[0].Nombre.Should().Be("Pop!");
    }

    [Fact]
    public async Task Handle_MapeoCaracteristicasFunko_IdYNombreCorrectos()
    {
        // Mata mutantes: c.Id → 0, c.Nombre → null en CaracteristicasFunko
        _repo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>());
        _repo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());
        _repo.GetFunkoCaracteristicasAsync()
             .Returns(new List<FunkoCaracteristica> { new() { Id = 9, Nombre = "Glow in the Dark" } });
        _repo.GetTcgFranquiciasAsync().Returns(new List<TcgFranquicia>());

        var result = await CrearHandler().Handle(new ObtenerCatalogosQuery(), CancellationToken.None);

        result.CaracteristicasFunko.Should().HaveCount(1);
        result.CaracteristicasFunko[0].Id.Should().Be(9);
        result.CaracteristicasFunko[0].Nombre.Should().Be("Glow in the Dark");
    }

    [Fact]
    public async Task Handle_MapeoFranquiciasTcg_IdYNombreCorrectos()
    {
        // Mata mutantes: f.Id → 0, f.Nombre → null en FranquiciasTcg
        _repo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>());
        _repo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());
        _repo.GetFunkoCaracteristicasAsync().Returns(new List<FunkoCaracteristica>());
        _repo.GetTcgFranquiciasAsync()
             .Returns(new List<TcgFranquicia> { new() { Id = 2, Nombre = "Pokémon" } });

        var result = await CrearHandler().Handle(new ObtenerCatalogosQuery(), CancellationToken.None);

        result.FranquiciasTcg.Should().HaveCount(1);
        result.FranquiciasTcg[0].Id.Should().Be(2);
        result.FranquiciasTcg[0].Nombre.Should().Be("Pokémon");
    }

    [Fact]
    public async Task Handle_TodasLasColeccionesVacias_RetornaDtoConListasVacias()
    {
        // Mata mutantes que asignan null en lugar de lista vacía
        _repo.GetHotWheelsCategoriasAsync().Returns(new List<HotWheelsCategoria>());
        _repo.GetFunkoSubtiposAsync().Returns(new List<FunkoSubtipo>());
        _repo.GetFunkoCaracteristicasAsync().Returns(new List<FunkoCaracteristica>());
        _repo.GetTcgFranquiciasAsync().Returns(new List<TcgFranquicia>());

        var result = await CrearHandler().Handle(new ObtenerCatalogosQuery(), CancellationToken.None);

        result.CategoriasHotWheels.Should().BeEmpty();
        result.SubtiposFunko.Should().BeEmpty();
        result.CaracteristicasFunko.Should().BeEmpty();
        result.FranquiciasTcg.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ObtenerExpansionesYPacksQueryHandler — 0% (5 survived, 2 no cov)
//
// Mutantes objetivo:
//   - e.Id, e.Nombre en Select de Expansiones
//   - p.Id, p.Nombre en Select de Packs
//   - request.FranquiciaId pasado a ambas queries
// ═══════════════════════════════════════════════════════════════════════════

public class ObtenerExpansionesYPacksQueryHandlerTests
{
    private readonly ICatalogoRepository _repo = Substitute.For<ICatalogoRepository>();

    private ObtenerExpansionesYPacksQueryHandler CrearHandler() => new(_repo);

    [Fact]
    public async Task Handle_MapeoExpansiones_IdYNombreCorrectos()
    {
        // Mata mutantes: e.Id → 0, e.Nombre → null
        _repo.GetTcgExpansionesAsync(1)
             .Returns(new List<TcgExpansion> { new() { Id = 15, Nombre = "Base Set" } });
        _repo.GetTcgPacksAsync(1).Returns(new List<TcgPack>());

        var result = await CrearHandler().Handle(
            new ObtenerExpansionesYPacksQuery(1), CancellationToken.None);

        result.Expansiones.Should().HaveCount(1);
        result.Expansiones[0].Id.Should().Be(15);
        result.Expansiones[0].Nombre.Should().Be("Base Set");
    }

    [Fact]
    public async Task Handle_MapeoPacks_IdYNombreCorrectos()
    {
        // Mata mutantes: p.Id → 0, p.Nombre → null
        _repo.GetTcgExpansionesAsync(2).Returns(new List<TcgExpansion>());
        _repo.GetTcgPacksAsync(2)
             .Returns(new List<TcgPack> { new() { Id = 8, Nombre = "Booster Box" } });

        var result = await CrearHandler().Handle(
            new ObtenerExpansionesYPacksQuery(2), CancellationToken.None);

        result.Packs.Should().HaveCount(1);
        result.Packs[0].Id.Should().Be(8);
        result.Packs[0].Nombre.Should().Be("Booster Box");
    }

    [Fact]
    public async Task Handle_PasaFranquiciaIdCorrectoAmbosRepositorios()
    {
        // Mata mutante: request.FranquiciaId → 0 en cualquiera de los dos calls
        _repo.GetTcgExpansionesAsync(Arg.Any<int>()).Returns(new List<TcgExpansion>());
        _repo.GetTcgPacksAsync(Arg.Any<int>()).Returns(new List<TcgPack>());

        await CrearHandler().Handle(
            new ObtenerExpansionesYPacksQuery(42), CancellationToken.None);

        await _repo.Received(1).GetTcgExpansionesAsync(42);
        await _repo.Received(1).GetTcgPacksAsync(42);
    }

    [Fact]
    public async Task Handle_SinDatos_RetornaDtoConListasVacias()
    {
        _repo.GetTcgExpansionesAsync(Arg.Any<int>()).Returns(new List<TcgExpansion>());
        _repo.GetTcgPacksAsync(Arg.Any<int>()).Returns(new List<TcgPack>());

        var result = await CrearHandler().Handle(
            new ObtenerExpansionesYPacksQuery(1), CancellationToken.None);

        result.Expansiones.Should().BeEmpty();
        result.Packs.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SincronizarCatalogosCommandHandler — 0% (48 survived, 35 no cov)
//
// Mutantes objetivo:
//   - franquicia is null      → par null/no-null + continue
//   - expansionApi.ImageUrl is not null → par ambas ramas
//   - existente is null       → rama Add vs Update
//   - existente.Nombre != expansionApi.Nombre → operador !=
//   - imageName is not null && existente.ImageUrl != imageName → compound
//   - cambio = true           → false (no se actualiza)
//   - totalAgregadas++        → no incrementa
//   - totalActualizadas++     → no incrementa
//   - errores.Add(msg)        → no agrega error
//   - Result fields: TotalAgregadas, TotalActualizadas, Errores
// ═══════════════════════════════════════════════════════════════════════════

public class SincronizarCatalogosCommandHandlerTests
{
    private readonly ICatalogoRepository _catalogoRepo = Substitute.For<ICatalogoRepository>();
    private readonly IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private ITcgCatalogoApiService CrearApiService(
        string franquicia,
        List<ExpansionApiResult> expansiones)
    {
        var svc = Substitute.For<ITcgCatalogoApiService>();
        svc.FranquiciaNombre.Returns(franquicia);
        svc.GetExpansionesAsync().Returns(expansiones);
        return svc;
    }

    private SincronizarCatalogosCommandHandler CrearHandler(
        params ITcgCatalogoApiService[] servicios)
        => new(_catalogoRepo, servicios, _imageStorage, _logger);

    // ── franquicia is null → errores.Add + continue ───────────────────────────

    [Fact]
    public async Task Handle_FranquiciaNoEncontrada_AgregaErrorYContinua()
    {
        // Mata mutante: franquicia is null → franquicia is not null
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Pokémon").Returns((TcgFranquicia?)null);

        var svc = CrearApiService("Pokémon", new List<ExpansionApiResult>());
        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.Errores.Should().HaveCount(1);
        result.Errores[0].Should().Contain("Pokémon");
        result.TotalAgregadas.Should().Be(0);
    }

    [Fact]
    public async Task Handle_FranquiciaEncontrada_NoAgregaError()
    {
        // Par complementario
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync(Arg.Any<string>(), Arg.Any<int>())
                     .Returns((TcgExpansion?)null);
        _catalogoRepo.AddTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", null)
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.Errores.Should().BeEmpty();
    }

    // ── existente is null → AddTcgExpansionAsync + totalAgregadas++ ──────────

    [Fact]
    public async Task Handle_ExpansionNueva_LlamaAddYIncrementaTotalAgregadas()
    {
        // Mata mutante: existente is null → invertido (nunca agrega)
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync("Alpha", 1)
                     .Returns((TcgExpansion?)null);
        _catalogoRepo.AddTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", null)
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.TotalAgregadas.Should().Be(1);
        await _catalogoRepo.Received(1).AddTcgExpansionAsync(Arg.Any<TcgExpansion>());
    }

    [Fact]
    public async Task Handle_DosExpansionesNuevas_TotalAgregadasEsDos()
    {
        // Mata mutante: totalAgregadas++ → no incrementa
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync(Arg.Any<string>(), Arg.Any<int>())
                     .Returns((TcgExpansion?)null);
        _catalogoRepo.AddTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", null),
            new("Beta", null)
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.TotalAgregadas.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ExpansionNueva_AsignaFranquiciaIdCorrectamente()
    {
        // Mata mutante: FranquiciaId = franquicia.Id → FranquiciaId = 0
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Pokémon")
                     .Returns(new TcgFranquicia { Id = 5, Nombre = "Pokémon" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync(Arg.Any<string>(), Arg.Any<int>())
                     .Returns((TcgExpansion?)null);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        TcgExpansion? capturada = null;
        _catalogoRepo.AddTcgExpansionAsync(Arg.Do<TcgExpansion>(e => capturada = e))
                     .Returns(Task.CompletedTask);

        var svc = CrearApiService("Pokémon", new List<ExpansionApiResult>
        {
            new("Base Set", null)
        });

        await CrearHandler(svc).Handle(new SincronizarCatalogosCommand(), CancellationToken.None);

        capturada!.FranquiciaId.Should().Be(5);
        capturada.Nombre.Should().Be("Base Set");
    }

    // ── existente no null → rama Update ──────────────────────────────────────

    [Fact]
    public async Task Handle_ExpansionExistenteSinCambios_NoLlamaUpdate()
    {
        // Si nombre e imagen son iguales, cambio=false → no se actualiza
        var existente = new TcgExpansion { Id = 1, Nombre = "Alpha", FranquiciaId = 1, ImageUrl = null };
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync("Alpha", 1).Returns(existente);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", null)
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        await _catalogoRepo.DidNotReceive().UpdateTcgExpansionAsync(Arg.Any<TcgExpansion>());
        result.TotalActualizadas.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ExpansionExistenteConNombreDiferente_LlamaUpdateEIncrementaActualizadas()
    {
        // Mata mutante: existente.Nombre != expansionApi.Nombre → ==
        var existente = new TcgExpansion { Id = 1, Nombre = "Alpha Antiguo", FranquiciaId = 1, ImageUrl = null };
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync("Alpha Nuevo", 1).Returns(existente);
        _catalogoRepo.UpdateTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync(Arg.Any<string>()).Returns((string?)null);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha Nuevo", null)
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.TotalActualizadas.Should().Be(1);
        await _catalogoRepo.Received(1).UpdateTcgExpansionAsync(Arg.Any<TcgExpansion>());
    }

    [Fact]
    public async Task Handle_ExpansionExistenteConNuevoImageName_LlamaUpdateEIncrementaActualizadas()
    {
        // Mata mutante: imageName is not null && ... → siempre false
        var existente = new TcgExpansion { Id = 1, Nombre = "Alpha", FranquiciaId = 1, ImageUrl = null };
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync("Alpha", 1).Returns(existente);
        _catalogoRepo.UpdateTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync("http://img.png").Returns("local/img.png");

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", "http://img.png")
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.TotalActualizadas.Should().Be(1);
        await _catalogoRepo.Received(1).UpdateTcgExpansionAsync(
            Arg.Is<TcgExpansion>(e => e.ImageUrl == "local/img.png"));
    }

    [Fact]
    public async Task Handle_ExpansionExistenteConMismaImagen_NoCambia()
    {
        // Mata mutante: existente.ImageUrl != imageName → siempre true
        var existente = new TcgExpansion { Id = 1, Nombre = "Alpha", FranquiciaId = 1, ImageUrl = "local/img.png" };
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync("Alpha", 1).Returns(existente);
        _imageStorage.SaveImageFromUrlAsync("http://img.png").Returns("local/img.png");

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", "http://img.png")
        });

        var result = await CrearHandler(svc).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        await _catalogoRepo.DidNotReceive().UpdateTcgExpansionAsync(Arg.Any<TcgExpansion>());
        result.TotalActualizadas.Should().Be(0);
    }

    // ── expansionApi.ImageUrl is not null → SaveImageFromUrl ─────────────────

    [Fact]
    public async Task Handle_ImageUrlNoNull_LlamaSaveImageFromUrl()
    {
        // Mata mutante: ImageUrl is not null → false (nunca guarda imagen)
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync(Arg.Any<string>(), Arg.Any<int>())
                     .Returns((TcgExpansion?)null);
        _catalogoRepo.AddTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);
        _imageStorage.SaveImageFromUrlAsync("http://example.com/img.svg").Returns("local/img.png");

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", "http://example.com/img.svg")
        });

        await CrearHandler(svc).Handle(new SincronizarCatalogosCommand(), CancellationToken.None);

        await _imageStorage.Received(1).SaveImageFromUrlAsync("http://example.com/img.svg");
    }

    [Fact]
    public async Task Handle_ImageUrlNull_NoLlamaSaveImageFromUrl()
    {
        // Mata mutante: ImageUrl is not null → true (siempre guarda)
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Magic" });
        _catalogoRepo.GetTcgExpansionByNombreYFranquiciaAsync(Arg.Any<string>(), Arg.Any<int>())
                     .Returns((TcgExpansion?)null);
        _catalogoRepo.AddTcgExpansionAsync(Arg.Any<TcgExpansion>()).Returns(Task.CompletedTask);

        var svc = CrearApiService("Magic", new List<ExpansionApiResult>
        {
            new("Alpha", null)
        });

        await CrearHandler(svc).Handle(new SincronizarCatalogosCommand(), CancellationToken.None);

        await _imageStorage.DidNotReceive().SaveImageFromUrlAsync(Arg.Any<string>());
    }

    // ── Excepción por servicio → errores.Add + continúa con el siguiente ─────

    [Fact]
    public async Task Handle_ExcepcionEnUnServicio_AgregaErrorYContinuaConOtro()
    {
        // Mata mutante: errores.Add(msg) → eliminado
        var svcFalla = Substitute.For<ITcgCatalogoApiService>();
        svcFalla.FranquiciaNombre.Returns("Pokémon");
        svcFalla.When(x => x.GetExpansionesAsync()).Throw(new Exception("API caída"));

        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Pokémon")
                     .Returns(new TcgFranquicia { Id = 1, Nombre = "Pokémon" });

        var svcOk = CrearApiService("Magic", new List<ExpansionApiResult>());
        _catalogoRepo.GetTcgFranquiciaByNombreAsync("Magic")
                     .Returns(new TcgFranquicia { Id = 2, Nombre = "Magic" });

        var result = await CrearHandler(svcFalla, svcOk).Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.Errores.Should().HaveCount(1);
        result.Errores[0].Should().Contain("Pokémon");
        // Magic sí fue procesado (no se detuvo por la excepción de Pokémon)
        await _catalogoRepo.Received(1).GetTcgFranquiciaByNombreAsync("Magic");
    }

    // ── Result final: campos TotalAgregadas, TotalActualizadas, Errores ───────

    [Fact]
    public async Task Handle_SinServicios_RetornaResultadoEnCero()
    {
        // Mata mutantes en los 3 campos del record de resultado
        var result = await CrearHandler().Handle(
            new SincronizarCatalogosCommand(), CancellationToken.None);

        result.TotalAgregadas.Should().Be(0);
        result.TotalActualizadas.Should().Be(0);
        result.Errores.Should().BeEmpty();
    }
}