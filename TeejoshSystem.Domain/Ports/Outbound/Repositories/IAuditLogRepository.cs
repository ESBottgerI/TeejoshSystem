using TeejoshSystem.Domain.Entities;

namespace TeejoshSystem.Domain.Ports.Outbound.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> ConsultarAsync(string? entidad, string? usuario, int limite = 200);
    }
}