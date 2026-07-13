namespace TeejoshSystem.Domain.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida para suscripciones a cambios en tiempo real vía Supabase Realtime.
    /// La implementación usa WebSocket raw con protocolo Phoenix Channels (sin SDK oficial).
    /// Blazor Admin hereda este mismo contrato — un port único para ambas UIs.
    /// </summary>
    public interface IRealtimeService
    {
        /// <summary>True si el WebSocket está conectado y autenticado con Supabase.</summary>
        bool IsConnected { get; }

        /// <summary>
        /// Suscribe a cambios INSERT/UPDATE/DELETE en una tabla PostgreSQL.
        /// El callback recibe el evento completo con el payload JSON del registro afectado.
        /// Se puede llamar múltiples veces para suscribirse a distintas tablas.
        /// </summary>
        Task SubscribeAsync(
            string table,
            Action<RealtimeEvent> onEvent,
            CancellationToken ct = default);

        /// <summary>Cancela todas las suscripciones activas y cierra el WebSocket limpiamente.</summary>
        Task UnsubscribeAllAsync();
    }

    public sealed record RealtimeEvent(
        /// <summary>INSERT | UPDATE | DELETE</summary>
        string EventType,
        string Table,
        string Schema,
        /// <summary>JSON del registro afectado (campo "new" para INSERT/UPDATE, "old" para DELETE)</summary>
        string PayloadJson
    );
}