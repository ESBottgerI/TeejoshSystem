using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.CambiarPassword;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.DesactivarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.RegistrarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Domain.Ports.Outbound.Auth;

namespace TeejoshSystem.Application.Tests.Auth;

// ═══════════════════════════════════════════════════════════════════════════
// AutenticarUsuarioCommandHandler
//
// Mutantes objetivo:
//   - IsNullOrWhiteSpace(NombreUsuario) → reemplazado por false
//   - IsNullOrWhiteSpace(Password)      → reemplazado por false
//   - !resultado.Exitoso                → reemplazado por resultado.Exitoso
//   - Trim() sobre NombreUsuario        → eliminado por mutante
// ═══════════════════════════════════════════════════════════════════════════

public class AutenticarUsuarioCommandHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private AutenticarUsuarioCommandHandler CrearHandler()
        => new(_authService, _logger);

    // ── Flujo exitoso ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CredencialesValidas_RetornaSuccessConSesionDto()
    {
        _authService.AutenticarAsync("admin", "Pass1234!", Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "Pass1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NombreUsuario.Should().Be("admin");
        result.Value.Rol.Should().Be(RolUsuario.Administrador);
        result.Value.UsuarioId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_CredencialesValidas_InvocaAutenticarAsyncUnaVez()
    {
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador));

        await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "Pass1234!"), CancellationToken.None);

        await _authService.Received(1)
              .AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Guard: NombreUsuario vacío/nulo ───────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreUsuarioInvalido_RetornaFailureSinLlamarAuth(string? nombre)
    {
        // Mata el mutante que reemplaza IsNullOrWhiteSpace(NombreUsuario) por false
        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand(nombre!, "Pass1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
        await _authService.DidNotReceive()
              .AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Guard: Password vacío/nulo ────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_PasswordInvalido_RetornaFailureSinLlamarAuth(string? password)
    {
        // Mata el mutante que reemplaza IsNullOrWhiteSpace(Password) por false
        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", password!), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _authService.DidNotReceive()
              .AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Autenticación fallida — !resultado.Exitoso ────────────────────────────

    [Fact]
    public async Task Handle_AuthServiceRetornaInvalido_RetornaFailure()
    {
        // Mata el mutante que elimina el ! de !resultado.Exitoso
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Invalido("Credenciales inválidas."));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "WrongPass!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Credenciales inválidas.");
    }

    [Fact]
    public async Task Handle_AuthServiceRetornaExitoso_RetornaSuccess()
    {
        // Test complementario: cuando Exitoso=true, el resultado es Success
        _authService.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(5, "operador", RolUsuario.Operador));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("operador", "Pass1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Rol.Should().Be(RolUsuario.Operador);
    }

    // ── Trim sobre NombreUsuario ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_NombreConEspacios_EnviaAlAuthServiceConTrim()
    {
        // Mata el mutante que elimina el .Trim() del NombreUsuario
        _authService.AutenticarAsync("admin", Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(AutenticacionResultado.Valido(1, "admin", RolUsuario.Administrador));

        await CrearHandler().Handle(
            new AutenticarUsuarioCommand("  admin  ", "Pass1234!"), CancellationToken.None);

        // El handler debe pasar "admin" (sin espacios), no "  admin  "
        await _authService.Received(1)
              .AutenticarAsync("admin", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Excepción inesperada ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionEnAuthService_RetornaFailure()
    {
        _authService.When(x => x.AutenticarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                    .Throw(new Exception("Error de red"));

        var result = await CrearHandler().Handle(
            new AutenticarUsuarioCommand("admin", "Pass1234!"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RegistrarUsuarioCommandHandler
//
// Mutantes objetivo:
//   - IsNullOrWhiteSpace(NombreUsuario)
//   - IsNullOrWhiteSpace(Password) || Password.Length < 8  → boundary 7 y 8
//   - await ExisteAsync(...)  → valor devuelto true/false
// ═══════════════════════════════════════════════════════════════════════════

public class RegistrarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private RegistrarUsuarioCommandHandler CrearHandler()
        => new(_repo, _logger);

    // ── Flujo exitoso ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DatosValidos_RetornaSuccessYLlamaCrearAsync()
    {
        _repo.ExisteAsync("nuevo", Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("nuevo", "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).CrearAsync("nuevo", "Pass1234!", RolUsuario.Operador, Arg.Any<CancellationToken>());
    }

    // ── Guard: NombreUsuario ──────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_NombreUsuarioInvalido_RetornaFailureSinPersistir(string? nombre)
    {
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand(nombre!, "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        await _repo.DidNotReceive().CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>());
    }

    // ── Guard: Password length < 8 — boundary crítico ────────────────────────

    [Fact]
    public async Task Handle_Password7Caracteres_RetornaFailure()
    {
        // Boundary inferior inválido: 7 caracteres — mata mutante < → <=
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pass12!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("8");
    }

    [Fact]
    public async Task Handle_Password8Caracteres_EsValido()
    {
        // Boundary superior válido: exactamente 8 caracteres
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pass123!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_PasswordVacioONulo_RetornaFailure(string? password)
    {
        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", password!, RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── Guard: usuario ya existe ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_UsuarioYaExiste_RetornaFailureSinCrear()
    {
        // Mata el mutante que invierte la condición de ExisteAsync
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
    public async Task Handle_UsuarioNoExiste_ProcedaCrear()
    {
        // Complementario: cuando ExisteAsync=false, sí debe crear
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.CrearAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("nuevo", "Pass1234!", RolUsuario.Administrador),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>());
    }

    // ── Excepción inesperada ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_RetornaFailure()
    {
        _repo.ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _repo.When(x => x.CrearAsync(Arg.Any<string>(), Arg.Any<string>(),
                         Arg.Any<RolUsuario>(), Arg.Any<CancellationToken>()))
             .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new RegistrarUsuarioCommand("usuario", "Pass1234!", RolUsuario.Operador),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// CambiarPasswordCommandHandler
//
// Mutantes objetivo:
//   - PasswordNuevo.Length < 8            → boundary 7 (fail) y 8 (pass)
//   - PasswordActual == PasswordNuevo     → operador == → !=
//   - !passwordValida                     → eliminar el !
// ═══════════════════════════════════════════════════════════════════════════

public class CambiarPasswordCommandHandlerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private CambiarPasswordCommandHandler CrearHandler()
        => new(_authService, _repo, _logger);

    // ── Flujo exitoso ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DatosValidos_RetornaSuccessYActualizaPassword()
    {
        _authService.VerificarPasswordAsync(1, "Actual123!", Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(1, "Nuevo1234!", Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nuevo1234!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).ActualizarPasswordAsync(1, "Nuevo1234!", Arg.Any<CancellationToken>());
    }

    // ── Guard: PasswordNuevo length < 8 — boundary ───────────────────────────

    [Fact]
    public async Task Handle_NuevoPassword7Caracteres_RetornaFailureSinVerificar()
    {
        // Boundary: 7 caracteres inválido — mata mutante < → <=
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nue12!X"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("8");
        await _authService.DidNotReceive()
              .VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NuevoPassword8Caracteres_EsValido()
    {
        // Boundary: 8 caracteres válido
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nuevo12!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Handle_NuevoPasswordVacioONulo_RetornaFailure(string? passwordNuevo)
    {
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", passwordNuevo!),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    // ── Guard: passwords iguales — operador == ────────────────────────────────

    [Fact]
    public async Task Handle_NuevoPasswordIgualAlActual_RetornaFailureSinVerificar()
    {
        // Mata el mutante que cambia == por !=
        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "MismoPass1!", "MismoPass1!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("distinta");
        await _authService.DidNotReceive()
              .VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NuevoPasswordDistintoAlActual_NoBloqueaPorIgualdad()
    {
        // Complementario: passwords distintos no deben ser bloqueados por el guard de igualdad
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Diferente1!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Guard: !passwordValida ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_PasswordActualIncorrecto_RetornaFailureSinActualizar()
    {
        // Mata el mutante que elimina el ! de !passwordValida
        _authService.VerificarPasswordAsync(1, "Incorrecto!", Arg.Any<CancellationToken>())
                    .Returns(false);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Incorrecto!", "Nuevo1234!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("incorrecta");
        await _repo.DidNotReceive()
              .ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PasswordActualCorrecto_ProcedaActualizar()
    {
        // Complementario: cuando verificación=true, sí actualiza
        _authService.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(true);
        _repo.ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Correcto1!", "Nuevo1234!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).ActualizarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Excepción inesperada ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ExcepcionEnVerificar_RetornaFailure()
    {
        _authService.When(x => x.VerificarPasswordAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
                    .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new CambiarPasswordCommand(1, "Actual123!", "Nuevo1234!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DesactivarUsuarioCommandHandler
//
// Flujo simple: delega a DesactivarAsync y retorna Success.
// Mutante: excepción → Failure en lugar de propagarse.
// ═══════════════════════════════════════════════════════════════════════════

public class DesactivarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IAppLogger _logger = Substitute.For<IAppLogger>();

    private DesactivarUsuarioCommandHandler CrearHandler()
        => new(_repo, _logger);

    [Fact]
    public async Task Handle_UsuarioValido_DesactivaYRetornaSuccess()
    {
        _repo.DesactivarAsync(5, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await CrearHandler().Handle(
            new DesactivarUsuarioCommand(5), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).DesactivarAsync(5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DesactivarAsync_EsInvocadoConElIdCorrecto()
    {
        // Mata mutante que pase un ID diferente al recibido
        _repo.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await CrearHandler().Handle(new DesactivarUsuarioCommand(42), CancellationToken.None);

        await _repo.Received(1).DesactivarAsync(42, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().DesactivarAsync(Arg.Is<int>(id => id != 42), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_RetornaFailure()
    {
        _repo.When(x => x.DesactivarAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()))
             .Throw(new Exception("Error de BD"));

        var result = await CrearHandler().Handle(
            new DesactivarUsuarioCommand(1), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ListarUsuariosQueryHandler
//
// Mutantes objetivo: projection Select — cada campo del UsuarioListaDto
// puede ser mutado (Id → 0, NombreUsuario → null, Rol → default, Activo → !Activo).
// ═══════════════════════════════════════════════════════════════════════════

public class ListarUsuariosQueryHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();

    private ListarUsuariosQueryHandler CrearHandler() => new(_repo);

    private static Usuario FabricarUsuario(int id, string nombre, RolUsuario rol, bool activo)
    {
        var u = new Usuario();
        // Setear via reflexión — las propiedades son private set
        typeof(Usuario).GetProperty("Id")!.SetValue(u, id);
        typeof(Usuario).GetProperty("NombreUsuario")!.SetValue(u, nombre);
        typeof(Usuario).GetProperty("Rol")!.SetValue(u, rol);
        typeof(Usuario).GetProperty("Activo")!.SetValue(u, activo);
        return u;
    }

    [Fact]
    public async Task Handle_ConUsuarios_MapeatodosLosCamposCorrectamente()
    {
        // Verifica que Id, NombreUsuario, Rol y Activo se mapean sin mutación
        var usuarios = new List<Usuario>
        {
            FabricarUsuario(7, "operador01", RolUsuario.Operador, true)
        };
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = (await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        var dto = result.Single();
        dto.Id.Should().Be(7);
        dto.NombreUsuario.Should().Be("operador01");
        dto.Rol.Should().Be(RolUsuario.Operador);
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsuarioInactivo_MapeatActivoComoFalse()
    {
        // Mata el mutante que invierte el campo Activo (!Activo en la projection)
        var usuarios = new List<Usuario>
        {
            FabricarUsuario(3, "dado_de_baja", RolUsuario.Operador, false)
        };
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = (await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None)).ToList();

        result.Single().Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_VariosUsuarios_RetornaUnDtoPorUsuario()
    {
        var usuarios = new List<Usuario>
        {
            FabricarUsuario(1, "admin",    RolUsuario.Administrador, true),
            FabricarUsuario(2, "vendedor", RolUsuario.Operador,       true),
            FabricarUsuario(3, "baja",     RolUsuario.Operador,       false)
        };
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = await CrearHandler().Handle(new ListarUsuariosQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SinUsuarios_RetornaListaVacia()
    {
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(new List<Usuario>());

        var result = await CrearHandler().Handle(new ListarUsuariosQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RolAdministrador_EsMapeadoCorrectamente()
    {
        // Mata mutante que reemplaza u.Rol por RolUsuario.Operador fijo
        var usuarios = new List<Usuario>
        {
            FabricarUsuario(1, "superadmin", RolUsuario.Administrador, true)
        };
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = (await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None)).ToList();

        result.Single().Rol.Should().Be(RolUsuario.Administrador);
    }
}