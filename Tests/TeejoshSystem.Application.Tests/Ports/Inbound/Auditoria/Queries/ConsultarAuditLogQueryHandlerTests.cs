using TeejoshSystem.Application.Ports.Inbound.Auditoria.Queries.ConsultarAuditLog;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Tests.Ports.Inbound.Auditoria.Queries;

public class ConsultarAuditLogQueryHandlerTests
{
    private readonly IAuditLogRepository _repository = Substitute.For<IAuditLogRepository>();

    private ConsultarAuditLogQueryHandler CrearHandler()
        => new(_repository);

    [Fact]
    public async Task Handle_SinFiltros_RetornaTodasLasEntradas()
    {
        var entradas = new List<AuditLog>
        {
            new("Producto", "1", "Crear", "admin", "{\"Nombre\":{\"anterior\":null,\"nuevo\":\"Hot Wheels\"}}"),
            new("Producto", "2", "Eliminar", "admin", "{\"Nombre\":{\"anterior\":\"Funko\",\"nuevo\":null}}")
        };

        _repository.ConsultarAsync(null, null)
            .Returns(entradas);

        var resultado = await CrearHandler().Handle(
            new ConsultarAuditLogQuery(), CancellationToken.None);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ConFiltroEntidad_LlamaRepositorioConEntidadCorrecta()
    {
        _repository.ConsultarAsync("Producto", null)
            .Returns(new List<AuditLog>
            {
                new("Producto", "1", "Crear", "admin", null)
            });

        var resultado = await CrearHandler().Handle(
            new ConsultarAuditLogQuery(Entidad: "Producto"), CancellationToken.None);

        resultado.Should().HaveCount(1);
        resultado[0].Entidad.Should().Be("Producto");

        await _repository.Received(1).ConsultarAsync("Producto", null);
    }

    [Fact]
    public async Task Handle_ConFiltroUsuario_LlamaRepositorioConUsuarioCorrecto()
    {
        _repository.ConsultarAsync(null, "admin")
            .Returns(new List<AuditLog>
            {
                new("Venta", "5", "Crear", "admin", null)
            });

        var resultado = await CrearHandler().Handle(
            new ConsultarAuditLogQuery(Usuario: "admin"), CancellationToken.None);

        resultado.Should().HaveCount(1);
        resultado[0].Usuario.Should().Be("admin");

        await _repository.Received(1).ConsultarAsync(null, "admin");
    }

    [Fact]
    public async Task Handle_MapeoDto_ContieneLosDatosCorrectos()
    {
        var timestamp = DateTime.UtcNow;

        _repository.ConsultarAsync(null, null)
            .Returns(new List<AuditLog>
            {
                new("Producto", "42", "Crear", "admin", "{\"cambio\":true}")
            });

        var resultado = await CrearHandler().Handle(
            new ConsultarAuditLogQuery(), CancellationToken.None);

        var dto = resultado[0];
        dto.Entidad.Should().Be("Producto");
        dto.EntidadId.Should().Be("42");
        dto.Accion.Should().Be("Crear");
        dto.Usuario.Should().Be("admin");
        dto.Cambios.Should().Be("{\"cambio\":true}");
    }

    [Fact]
    public async Task Handle_RepositorioVacio_RetornaListaVacia()
    {
        _repository.ConsultarAsync(null, null)
            .Returns(new List<AuditLog>());

        var resultado = await CrearHandler().Handle(
            new ConsultarAuditLogQuery(), CancellationToken.None);

        resultado.Should().BeEmpty();
    }
}