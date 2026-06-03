using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auth.Commands;

public class AutenticarUsuarioCommandHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private AutenticarUsuarioCommandHandler CrearHandler() => new(_authService, _logger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreUsuarioInvalido_RetornaFailureSinLlamarAuth(string? nombre)
    {
        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand(nombre!, "Pass123!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("nombre de usuario");
        await _authService.DidNotReceive().AutenticarAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_PasswordInvalido_RetornaFailureSinLlamarAuth(string? password)
    {
        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", password!), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("contraseña");
        await _authService.DidNotReceive().AutenticarAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NombreConEspacios_EnviaConTrimAplicado()
    {
        _authService.AutenticarAsync("admin", Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador));

        await CrearHandler().Handle(
            new AutenticarUsuarioCommand("  admin  ", "Pass123!"), CancellationToken.None);

        await _authService.Received(1)
              .AutenticarAsync("admin", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AuthRetornaInvalido_RetornaFailure()
    {
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Invalido("Credenciales inválidas."));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "wrong"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AuthRetornaValido_RetornaSuccess()
    {
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "Pass123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AuthInvalidoConMensaje_UsaMensajeDelServicio()
    {
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Invalido("Usuario inactivo."));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "pass"), CancellationToken.None);

        result.Error.Should().Be("Usuario inactivo.");
    }

    [Fact]
    public async Task Handle_AuthInvalidoConMensajeNull_UsaFallback()
    {
        var sinMensaje = new AutenticacionResultado(false, null, null, null, null);
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(sinMensaje);

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "pass"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Credenciales inválidas.");
    }

    [Fact]
    public async Task Handle_AuthExitoso_MapeaTodosLosCamposDelSesionDto()
    {
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(42, "operador", RolUsuario.Operador));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("operador", "Pass123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UsuarioId.Should().Be(42);
        result.Value.NombreUsuario.Should().Be("operador");
        result.Value.Rol.Should().Be(RolUsuario.Operador);
    }

    [Fact]
    public async Task Handle_ExcepcionEnAuthService_RetornaFailure()
    {
        _authService.When(x => x.AutenticarAsync(
                        Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                    .Throw(new Exception("Timeout de BD"));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "Pass123!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
