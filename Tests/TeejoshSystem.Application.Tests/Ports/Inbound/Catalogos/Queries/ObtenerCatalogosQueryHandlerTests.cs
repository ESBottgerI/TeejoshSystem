using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerCatalogos;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Catalogos.Queries;

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
