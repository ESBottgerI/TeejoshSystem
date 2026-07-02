using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Ports.Outbound.Repositories;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly InventarioDbContext _context;

        public AuditLogRepository(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> ConsultarAsync(
            string? entidad, string? usuario, int limite = 200)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entidad))
                query = query.Where(a => a.Entidad == entidad);

            if (!string.IsNullOrWhiteSpace(usuario))
                query = query.Where(a => a.Usuario == usuario);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(limite)
                .ToListAsync();
        }
    }
}