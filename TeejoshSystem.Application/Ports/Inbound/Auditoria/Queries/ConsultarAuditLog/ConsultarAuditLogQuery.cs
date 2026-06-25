using MediatR;

namespace TeejoshSystem.Application.Ports.Inbound.Auditoria.Queries.ConsultarAuditLog
{
    public record ConsultarAuditLogQuery(
        string? Entidad = null,
        string? Usuario = null,
        int Pagina = 1,
        int TamanioPagina = 50
    ) : IRequest<List<AuditLogDto>>;

    public record AuditLogDto(
        int Id,
        DateTime Timestamp,
        string? Usuario,
        string Entidad,
        string EntidadId,
        string Accion,
        string? Cambios
    );
}