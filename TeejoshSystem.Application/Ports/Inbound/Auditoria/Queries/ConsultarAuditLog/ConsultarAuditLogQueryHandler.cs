using MediatR;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;

namespace TeejoshSystem.Application.Ports.Inbound.Auditoria.Queries.ConsultarAuditLog
{
    public class ConsultarAuditLogQueryHandler
        : IRequestHandler<ConsultarAuditLogQuery, List<AuditLogDto>>
    {
        private readonly IAuditLogRepository _repository;

        public ConsultarAuditLogQueryHandler(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AuditLogDto>> Handle(
            ConsultarAuditLogQuery request,
            CancellationToken cancellationToken)
        {
            var resultado = await _repository.ConsultarAsync(
                request.Entidad, request.Usuario);

            return resultado.Select(a => new AuditLogDto(
                a.Id, a.Timestamp, a.Usuario,
                a.Entidad, a.EntidadId, a.Accion, a.Cambios))
                .ToList();
        }
    }
}