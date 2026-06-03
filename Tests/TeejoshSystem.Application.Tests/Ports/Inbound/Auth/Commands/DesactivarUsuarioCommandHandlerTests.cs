using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auth.Commands;

public class DesactivarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private DesactivarUsuarioCommandHandler CrearHandler() => new(_repo, _logger);

    [Fact]
    public async Task Handle_UsuarioValido_RetornaSuccess()
    {
        _repo.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new DesactivarUsuarioCommand(5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DesactivarAsync_RecibeLosArgumentosCorrectos()
    {
        _repo.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(new DesactivarUsuarioCommand(42), CancellationToken.None);

        await _repo.Received(1).DesactivarAsync(42, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().DesactivarAsync(
            Arg.Is<int>(id => id != 42), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DesactivarAsync_RecibeCancellationToken()
    {
        var cts = new CancellationTokenSource();
        _repo.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(new DesactivarUsuarioCommand(1), cts.Token);

        await _repo.Received(1).DesactivarAsync(Arg.Any<int>(), cts.Token);
    }

    [Fact]
    public async Task Handle_DesactivacionExitosa_LoguearaInfoConUsuarioId()
    {
        _repo.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(new DesactivarUsuarioCommand(99), CancellationToken.None);

        _logger.Received(1).Info(Arg.Is<string>(s => s.Contains("99")));
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_RetornaFailureConMensajeCorrecto()
    {
        _repo.When(x => x.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()))
             .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new DesactivarUsuarioCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Error al desactivar el usuario.");
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_LoguearaErrorConUsuarioId()
    {
        _repo.When(x => x.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()))
             .Throw(new Exception("Error"));

        await CrearHandler().Handle(new DesactivarUsuarioCommand(55), CancellationToken.None);

        _logger.Received(1).Error(
            Arg.Is<string>(s => s.Contains("55")), Arg.Any<Exception>());
    }
}