using Microsoft.EntityFrameworkCore;
using TeejoshSystem.Domain.Entities;
using TeejoshSystem.Domain.Ports.Outbound;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Audit
{
    public class DatabaseAuditLogger : IAuditLogger
    {
        private readonly InventarioDbContext _context;

        public DatabaseAuditLogger(InventarioDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(IEnumerable<AuditLogEntryData> entradas)
        {
            foreach (var entrada in entradas)
            {
                var log = new AuditLog(
                    entrada.Entidad,
                    entrada.EntidadId,
                    entrada.Accion.ToString(),
                    entrada.Usuario,
                    entrada.Cambios);

                _context.AuditLogs.Add(log);
            }

            await Task.CompletedTask;
        }
    }
}