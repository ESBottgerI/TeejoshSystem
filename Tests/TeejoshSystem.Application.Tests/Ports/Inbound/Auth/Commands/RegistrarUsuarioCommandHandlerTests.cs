using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auth.Commands;

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
