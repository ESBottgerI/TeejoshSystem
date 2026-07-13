namespace TeejoshSystem.Domain.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida para la cola de operaciones offline pendientes de sincronizar.
    /// El outbox vive SOLO en SQLite local — nunca en Supabase.
    /// </summary>
    public interface ISyncOutboxRepository
    {
        /// <summary>Encola una operación para sincronizar cuando haya conexión.</summary>
        Task EnqueueAsync(SyncOutboxEntry entry, CancellationToken ct = default);

        /// <summary>Retorna todas las entradas pendientes ordenadas por CreatedAt ASC (FIFO).</summary>
        Task<IReadOnlyList<SyncOutboxEntry>> GetPendingAsync(CancellationToken ct = default);

        /// <summary>Elimina una entrada ya sincronizada correctamente.</summary>
        Task MarkSyncedAsync(Guid entryId, CancellationToken ct = default);

        /// <summary>Incrementa RetryCount y guarda el último error sin eliminar la entrada.</summary>
        Task MarkFailedAsync(Guid entryId, string error, CancellationToken ct = default);

        /// <summary>Cantidad de entradas pendientes (para mostrar badge en UI).</summary>
        Task<int> CountPendingAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// Registro de una operación de escritura realizada offline que debe replicarse en Supabase.
    /// </summary>
    public class SyncOutboxEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>INSERT | UPDATE | DELETE</summary>
        public string OperationType { get; set; } = null!;

        /// <summary>Nombre de la tabla destino en PostgreSQL (e.g. "product", "sale", "sale_detail")</summary>
        public string EntityTable { get; set; } = null!;

        /// <summary>Id del registro afectado (para UPDATE/DELETE; null en INSERT antes de conocer el id remoto)</summary>
        public int? EntityId { get; set; }

        /// <summary>Payload JSON serializado listo para enviar a la REST API de Supabase</summary>
        public string PayloadJson { get; set; } = null!;

        /// <summary>Timestamp UTC de cuando se generó la operación offline</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Id único del dispositivo que generó la operación.
        /// Permite identificar el origen en conflictos multi-caja futuros.
        /// </summary>
        public string DeviceId { get; set; } = null!;

        /// <summary>Número de intentos de sincronización fallidos (para backoff/alerta)</summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>Último mensaje de error registrado; null si nunca falló</summary>
        public string? LastError { get; set; }
    }
}