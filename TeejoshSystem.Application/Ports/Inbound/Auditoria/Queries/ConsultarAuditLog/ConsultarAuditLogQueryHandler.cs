using MediatR;
using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Application.Ports.Inbound.Auditoria.Queries.ConsultarAuditLog
{
    public class ConsultarAuditLogQueryHandler
        : IRequestHandler<ConsultarAuditLogQuery, List<AuditLogDto>>
    {
        private readonly InventarioDbContext _context;

        public ConsultarAuditLogQueryHandler(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> Handle(
            ConsultarAuditLogQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Entidad))
                query = query.Where(a => a.Entidad == request.Entidad);

            if (!string.IsNullOrWhiteSpace(request.Usuario))
                query = query.Where(a => a.Usuario == request.Usuario);

            var resultado = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.Pagina - 1) * request.TamanioPagina)
                .Take(request.TamanioPagina)
                .Select(a => new AuditLogDto(
                    a.Id, a.Timestamp, a.Usuario,
                    a.Entidad, a.EntidadId, a.Accion, a.Cambios))
                .ToListAsync(cancellationToken);

            return resultado;
        }
    }
}