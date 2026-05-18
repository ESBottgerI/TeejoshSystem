using Microsoft.Extensions.Logging;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Logging
{
    /// <summary>
    /// Adaptador de salida que implementa IAppLogger delegando en
    /// Microsoft.Extensions.Logging. El destino real (consola, archivo,
    /// Supabase, etc.) se configura en el host mediante appsettings.json
    /// y la inyección de providers — este adaptador es agnóstico al destino.
    ///
    /// En desarrollo: Serilog → consola + archivo .log local.
    /// En producción (VPS): Serilog → archivo + sink opcional externo.
    /// </summary>
    public sealed class AppLogger : IAppLogger
    {
        private readonly ILogger<AppLogger> _logger;

        public AppLogger(ILogger<AppLogger> logger)
        {
            _logger = logger;
        }

        public void Info(string message)
            => _logger.LogInformation("{Message}", message);

        public void Warning(string message)
            => _logger.LogWarning("{Message}", message);

        public void Error(string message, Exception? ex = null)
        {
            if (ex is not null)
                _logger.LogError(ex, "{Message}", message);
            else
                _logger.LogError("{Message}", message);
        }

        public void Debug(string message)
            => _logger.LogDebug("{Message}", message);
    }
}