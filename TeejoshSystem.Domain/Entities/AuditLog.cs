namespace TeejoshSystem.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string? Usuario { get; private set; }
        public string Entidad { get; private set; } = null!;
        public string EntidadId { get; private set; } = null!;
        public string Accion { get; private set; } = null!;
        public string? Cambios { get; private set; }

        private AuditLog() { } // Para EF

        public AuditLog(
            string entidad,
            string entidadId,
            string accion,
            string? usuario,
            string? cambios)
        {
            Timestamp = DateTime.UtcNow;
            Entidad = entidad;
            EntidadId = entidadId;
            Accion = accion;
            Usuario = usuario;
            Cambios = cambios;
        }
    }
}