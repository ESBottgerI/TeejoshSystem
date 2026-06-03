using global::TeejoshSystem.Application.Ports.Inbound.Auth.Queries.ListarUsuarios;
using global::TeejoshSystem.Domain.Entities;
using global::TeejoshSystem.Domain.Enums;
using global::TeejoshSystem.Domain.Ports.Outbound.Auth;


namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auth.Queries;

/// <summary>
/// Tests de ListarUsuariosQueryHandler.
/// Constructor: (IUsuarioRepository)
///
/// Mutantes objetivo (2 survived + 2 no cov):
///   - u.Id       → 0           (projection arg 1)
///   - u.Activo   → !u.Activo   (projection arg 4)
///   - cancellationToken pasado a ListarAsync → default
///   - Select ejecutado → lista vacía devuelta directamente
/// </summary>

public class ListarUsuariosQueryHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();

    private ListarUsuariosQueryHandler CrearHandler() => new(_repo);

    // ── Mapeo: los 4 campos del UsuarioListaDto ───────────────────────────────

    [Fact]
    public async Task Handle_MapeoCompleto_TodosLosCamposCorrectos()
    {
        // Un solo test mata los 4 mutantes de la projection:
        // Id→0, NombreUsuario→null, Rol→default, Activo→!Activo
        var usuario = FabricarUsuario(7, "operador01", RolUsuario.Operador, true);
        _repo.ListarAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Usuario> { usuario });

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
    public async Task Handle_UsuarioInactivo_ActivoEsFalseNoTrue()
    {
        // Mata mutante: u.Activo → !u.Activo en la projection
        var usuario = FabricarUsuario(3, "dado_de_baja", RolUsuario.Operador, false);
        _repo.ListarAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Usuario> { usuario });

        var result = (await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None)).ToList();

        result.Single().Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RolAdministrador_EsMapeadoSinMutacion()
    {
        // Mata mutante: u.Rol → RolUsuario.Operador constante en la projection
        var admin = FabricarUsuario(1, "superadmin", RolUsuario.Administrador, true);
        _repo.ListarAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Usuario> { admin });

        var result = (await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None)).ToList();

        result.Single().Rol.Should().Be(RolUsuario.Administrador);
    }

    // ── cancellationToken pasado a ListarAsync ────────────────────────────────

    [Fact]
    public async Task Handle_PasaCancellationTokenAListarAsync()
    {
        // Mata mutante: ListarAsync(cancellationToken) → ListarAsync(default)
        var cts = new CancellationTokenSource();
        _repo.ListarAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Usuario>());

        await CrearHandler().Handle(new ListarUsuariosQuery(), cts.Token);

        await _repo.Received(1).ListarAsync(cts.Token);
    }

    // ── Select ejecutado correctamente ───────────────────────────────────────

    [Fact]
    public async Task Handle_VariosUsuarios_RetornaUnDtoPorCadaUno()
    {
        // Mata mutante que devuelve lista vacía ignorando Select
        var usuarios = new List<Usuario>
        {
            FabricarUsuario(1, "admin",    RolUsuario.Administrador, true),
            FabricarUsuario(2, "vendedor", RolUsuario.Operador,      true),
            FabricarUsuario(3, "baja",     RolUsuario.Operador,      false)
        };
        _repo.ListarAsync(Arg.Any<CancellationToken>()).Returns(usuarios);

        var result = await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SinUsuarios_RetornaEnumerableVacio()
    {
        _repo.ListarAsync(Arg.Any<CancellationToken>())
             .Returns(new List<Usuario>());

        var result = await CrearHandler().Handle(
            new ListarUsuariosQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    // ── Helper: reflexión porque Usuario tiene setters private ───────────────

    private static Usuario FabricarUsuario(
        int id, string nombre, RolUsuario rol, bool activo)
    {
        var u = new Usuario();
        typeof(Usuario).GetProperty("Id")!.SetValue(u, id);
        typeof(Usuario).GetProperty("NombreUsuario")!.SetValue(u, nombre);
        typeof(Usuario).GetProperty("Rol")!.SetValue(u, rol);
        typeof(Usuario).GetProperty("Activo")!.SetValue(u, activo);
        return u;
    }
}