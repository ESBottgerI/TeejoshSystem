namespace TeejoshSystem.Domain.Ports.Outbound
{
    /// <summary>
    /// Puerto de salida para logging técnico del aplicativo.
    /// Captura eventos que el audit log de BD no cubre:
    /// errores en APIs externas, fallos de autenticación,
    /// excepciones en handlers, tiempos de queries lentas, etc.
    /// </summary>
    public interface IAppLogger
    {
        /// <summary>Información general del flujo normal de la aplicación.</summary>
        void Info(string message);

        /// <summary>Situaciones anómalas pero recuperables.</summary>
        void Warning(string message);

        /// <summary>Errores que impiden completar una operación. Incluye la excepción si aplica.</summary>
        void Error(string message, Exception? ex = null);

        /// <summary>Información detallada útil sólo durante desarrollo/diagnóstico.</summary>
        void Debug(string message);
    }
}