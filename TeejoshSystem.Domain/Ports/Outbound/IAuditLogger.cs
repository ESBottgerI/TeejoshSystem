namespace TeejoshSystem.Domain.Ports.Outbound
{
    public enum AccionAuditoria
    {
        Crear,
        Actualizar,
        Eliminar
    }

    public record AuditLogEntryData(
        string Entidad,
        string EntidadId,
        AccionAuditoria Accion,
        string? Usuario,
        string? Cambios
    );

    /// <summary>
    /// Puerto de salida para trazabilidad de negocio.
    /// Distinto a IAppLogger: este registra QUÉ cambió en los datos y QUIÉN lo hizo,
    /// no eventos técnicos.
    /// </summary>
    public interface IAuditLogger
    {
        Task RegistrarAsync(IEnumerable<AuditLogEntryData> entradas);
    }
}