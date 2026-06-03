using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Productos.Commands.EliminarProducto;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Productos.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// EliminarProductoCommandHandler
// DeleteAsync recibe Producto, no int
// Command acepta List<int>
// ═══════════════════════════════════════════════════════════════════════════

public class EliminarProductoCommandHandlerTests
{
    private readonly IProductoRepository _repo;
    private readonly EliminarProductoCommandHandler _handler;

    public EliminarProductoCommandHandlerTests()
    {
        _repo = Substitute.For<IProductoRepository>();
        _handler = new EliminarProductoCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_IdsValidos_DebeDelgarADeleteRangeYRetornarSuccess()
    {
        _repo.DeleteRangeAsync(Arg.Any<IEnumerable<int>>())
             .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new EliminarProductoCommand(new List<int> { 1, 2 }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1)
                   .DeleteRangeAsync(Arg.Is<IEnumerable<int>>(ids =>
                       ids.SequenceEqual(new[] { 1, 2 })));
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DebeRetornarFailure()
    {
        // NSubstitute: When(...).Throw(...) para simular excepciones en async
        _repo.When(x => x.DeleteRangeAsync(Arg.Any<IEnumerable<int>>()))
             .Throw(new Exception("Error de BD"));

        var result = await _handler.Handle(
            new EliminarProductoCommand(new List<int> { 99 }),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
