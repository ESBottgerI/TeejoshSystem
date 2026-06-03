using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auth.Commands;

public class CambiarPasswordCommandHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private CambiarPasswordCommandHandler CrearHandler() => new(_authService, _repo, _logger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_PasswordNuevoVacioNuloOEspacios_RetornaFailureSinVerificar(string? passwordNuevo)
    {
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", passwordNuevo!), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _authService.DidNotReceive().VerificarPasswordAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PasswordNuevo7Caracteres_RetornaFailure()
    {
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nue12!X"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("8");
    }

    [Fact]
    public async Task Handle_PasswordNuevo8Caracteres_EsValido()
    {
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nuevo12!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PasswordCortoNoNulo_RetornaFailure()
    {
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nue1!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PasswordNuevoIgualAlActual_RetornaFailureSinVerificar()
    {
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Mismo1234!", "Mismo1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("distinta");
        await _authService.DidNotReceive().VerificarPasswordAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PasswordNuevoDistintoAlActual_NoBloqueaPorIgualdad()
    {
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Diferente1!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PasswordActualIncorrecto_RetornaFailureSinActualizar()
    {
        _authService.VerificarPasswordAsync(1, "Incorrecto!", Arg.Any<CancellationToken>())
                    .Returns(false);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Incorrecto!", "Nuevo1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("incorrecta");
        await _repo.DidNotReceive().ActualizarPasswordAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PasswordActualCorrecto_ProcedaActualizar()
    {
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Correcto1!", "Nuevo1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).ActualizarPasswordAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_VerificarPasswordAsync_RecibeLosArgumentosCorrectos()
    {
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CambiarPasswordCommand(7, "Actual123!", "Nuevo1234!"), CancellationToken.None);

        await _authService.Received(1).VerificarPasswordAsync(7, "Actual123!", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActualizarPasswordAsync_RecibeLosArgumentosCorrectos()
    {
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new CambiarPasswordCommand(7, "Actual123!", "Nuevo1234!"), CancellationToken.None);

        await _repo.Received(1).ActualizarPasswordAsync(7, "Nuevo1234!", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExcepcionEnVerificar_RetornaFailure()
    {
        _authService.When(x => x.VerificarPasswordAsync(
                        Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                    .Throw(new Exception("Timeout"));

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nuevo1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cambiar la contraseña");
    }
}