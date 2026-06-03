using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerExpansionesYPacks;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Catalogos.Queries;

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
