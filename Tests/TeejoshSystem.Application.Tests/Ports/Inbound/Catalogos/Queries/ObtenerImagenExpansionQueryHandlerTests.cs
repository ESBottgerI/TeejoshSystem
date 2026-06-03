using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Catalogos.Queries.ObtenerImagenExpansion;
using TeejoshSystem.Domain.Entities.Catalogos;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Catalogos.Queries;

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
