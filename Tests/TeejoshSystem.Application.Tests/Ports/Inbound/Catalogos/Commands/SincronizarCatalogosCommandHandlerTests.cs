using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Commands.SincronizarCatalogos;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Catalogos.Commands;

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