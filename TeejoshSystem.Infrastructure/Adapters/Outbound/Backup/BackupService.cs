using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Backup
{
    /// <summary>
    /// Servicio en segundo plano que copia inventario.db con timestamp cada N horas.
    /// Solo se activa cuando Database:Provider = "sqlite".
    /// </summary>
    public sealed class BackupService : BackgroundService
    {
        private readonly ILogger<BackupService> _logger;
        private readonly string _dbPath;
        private readonly string _backupFolder;
        private readonly TimeSpan _interval;
        private readonly int _maxBackups;

        public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
        {
            _logger = logger;

            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir  = Path.Combine(baseDir, "TeejoshSystem");

            _dbPath       = Path.Combine(appDir, "inventario.db");
            _backupFolder = Path.Combine(appDir, "backups");

            // Valores por defecto: cada 6 horas, conservar 10 backups
            var hours = configuration.GetValue<int>("Backup:IntervalHours", 6);
            _interval   = TimeSpan.FromHours(hours > 0 ? hours : 6);
            _maxBackups = configuration.GetValue<int>("Backup:MaxBackups", 10);
            if (_maxBackups < 1) _maxBackups = 1;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "BackupService iniciado. Intervalo: {Interval}h, máximo de backups: {Max}.",
                _interval.TotalHours, _maxBackups);

            Directory.CreateDirectory(_backupFolder);

            // Primer backup al arrancar, luego en intervalos regulares
            await RunBackupAsync(stoppingToken);

            using var timer = new PeriodicTimer(_interval);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunBackupAsync(stoppingToken);
            }
        }

        private async Task RunBackupAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_dbPath))
            {
                _logger.LogWarning("BackupService: no se encontró la base de datos en '{Path}'.", _dbPath);
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            var destName  = $"inventario_{timestamp}.db";
            var destPath  = Path.Combine(_backupFolder, destName);

            try
            {
                // FileShare.ReadWrite permite leer mientras SQLite tiene el archivo abierto
                await using var source      = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                await using var destination = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await source.CopyToAsync(destination, cancellationToken);

                _logger.LogInformation("Backup creado: {File}", destName);

                PurgeOldBackups();
            }
            catch (OperationCanceledException)
            {
                // Apagado limpio — no es un error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BackupService: error al crear el backup '{Dest}'.", destPath);

                // Eliminar archivo parcial si quedó
                if (File.Exists(destPath))
                    File.Delete(destPath);
            }
        }

        /// <summary>
        /// Elimina los backups más antiguos conservando solo los últimos <see cref="_maxBackups"/>.
        /// </summary>
        private void PurgeOldBackups()
        {
            try
            {
                var files = Directory
                    .GetFiles(_backupFolder, "inventario_*.db")
                    .OrderByDescending(f => f)   // orden lexicográfico == cronológico con formato yyyyMMdd_HHmm
                    .ToList();

                foreach (var file in files.Skip(_maxBackups))
                {
                    File.Delete(file);
                    _logger.LogInformation("Backup antiguo eliminado: {File}", Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BackupService: error al purgar backups antiguos.");
            }
        }
    }
}