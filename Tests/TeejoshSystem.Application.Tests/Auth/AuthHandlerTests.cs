using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Auth;

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

public class RegistrarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private RegistrarUsuarioCommandHandler CrearHandler() => new(_repo, _logger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreUsuarioInvalido_RetornaFailureSinConsultarRepo(string? nombre)
    {
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand(nombre!, "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("nombre de usuario");
        await _repo.DidNotReceive().ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_PasswordVacioNuloOEspacios_RetornaFailure(string? password)
    {
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", password!, RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("8");
    }

    [Fact]
    public async Task Handle_Password7Caracteres_RetornaFailure()
    {
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pass12!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("8");
    }

    [Fact]
    public async Task Handle_Password8Caracteres_EsValido()
    {
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pass123!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PasswordCortoNoNulo_RetornaFailure()
    {
        // Mata || → && : password válido (no null/whitespace) pero corto
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pas1!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UsuarioYaExiste_RetornaFailureSinCrear()
    {
        _repo.ExisteAsync("existente", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("existente", "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("uso");
        await _repo.DidNotReceive().CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_ProcedeCon()
    {
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("nuevo", "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DatosValidos_CrearAsyncRecibeLosArgumentosCorrectos()
    {
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        await CrearHandler().Handle(
            new RegistrarUsuarioCommand("nuevo_user", "Pass1234!", RolUsuario.Administrador),
            CancellationToken.None);

        await _repo.Received(1).CrearAsync(
            "nuevo_user", "Pass1234!", RolUsuario.Administrador, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExcepcionEnCrearAsync_RetornaFailure()
    {
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.When(x => x.CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
                         Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>()))
             .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("nuevo", "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error al registrar");
    }
}

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