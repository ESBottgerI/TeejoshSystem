namespace TeejoshSystem.Domain.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida para detección de conectividad con Supabase.
    /// La implementación hace ping HTTP al healthcheck de Supabase cada N segundos.
    /// Disponible en Blazor y Avalonia — mismo contrato.
    /// </summary>
    public interface IConnectivityService
    {
        /// <summary>True si el último ping a Supabase fue exitoso.</summary>
        bool IsOnline { get; }

        /// <summary>
        /// Se dispara cada vez que el estado cambia (online?offline o viceversa).
        /// El bool indica el NUEVO estado: true = online.
        /// </summary>
        event Action<bool> ConnectivityChanged;

        /// <summary>
        /// Realiza un ping inmediato y actualiza IsOnline.
        /// Útil para forzar verificación antes de una operación crítica.
        /// </summary>
        Task<bool> CheckNowAsync(CancellationToken ct = default);
    }
}