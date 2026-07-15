using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Sync
{
    /// <summary>
    /// BackgroundService que escucha cambios de conectividad y, al reconectar,
    /// replica en Supabase (via REST API) todas las operaciones encoladas en el outbox.
    ///
    /// Estrategia de conflicto, last-write-wins por registro:
    ///   El payload incluye el campo updated_at generado en el dispositivo.
    ///   Supabase aplica un UPSERT: si el registro remoto tiene updated_at m�s reciente,
    ///   la operaci�n no sobreescribe (manejado via pol�tica RLS o trigger en PostgreSQL).
    ///   Para la fase actual (1 caja), last-write-wins es suficiente.
    ///   Al escalar a m�ltiples cajas, se agrega un trigger de conflicto en Supabase.
    ///
    /// Reintentos:
    ///   M�ximo 5 intentos por entrada. Tras 5 fallos la entrada se marca como "stuck"
    ///   (RetryCount >= 5) y el operador debe resolverla manualmente desde el panel admin.
    /// </summary>
    public class SyncService : BackgroundService
    {
        private const int MaxRetries = 5;

        private readonly IConnectivityService _connectivity;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient _http;
        private readonly string _supabaseRestUrl;   // https://{ref}.supabase.co/rest/v1
        private readonly string _supabaseServiceKey;
        private readonly string _deviceId;

        // Sem�foro para evitar sync concurrente si el ping llega mientras ya se sincroniza
        private readonly SemaphoreSlim _syncLock = new(1, 1);

        public SyncService(
            IConnectivityService connectivity,
            IServiceScopeFactory scopeFactory,
            string supabaseUrl,
            string supabaseServiceKey,
            string deviceId)
        {
            _connectivity = connectivity;
            _scopeFactory = scopeFactory;
            _supabaseServiceKey = supabaseServiceKey;
            _deviceId = deviceId;
            _supabaseRestUrl = $"{supabaseUrl.TrimEnd('/')}/rest/v1";

            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("apikey", supabaseServiceKey);
            _http.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {supabaseServiceKey}");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Suscribirse al evento de conectividad � al pasar a online, disparar sync
            _connectivity.ConnectivityChanged += async (isOnline) =>
            {
                if (isOnline && !stoppingToken.IsCancellationRequested)
                    await SyncPendingAsync(stoppingToken);
            };

            // Si la app arranca ya online y hay pendientes del arranque anterior, sincronizar
            _ = Task.Run(async () =>
            {
                // Esperar un momento para que el host termine de inicializar
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
                if (_connectivity.IsOnline)
                    await SyncPendingAsync(stoppingToken);
            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task SyncPendingAsync(CancellationToken ct)
        {
            if (!await _syncLock.WaitAsync(0, ct)) return; // Ya hay sync en curso

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var outbox = scope.ServiceProvider.GetRequiredService<ISyncOutboxRepository>();

                var pending = await outbox.GetPendingAsync(ct);
                if (pending.Count == 0) return;

                foreach (var entry in pending)
                {
                    if (ct.IsCancellationRequested) break;
                    if (entry.RetryCount >= MaxRetries) continue; // Entrada "stuck", skip

                    try
                    {
                        await ApplyEntryAsync(entry, ct);
                        await outbox.MarkSyncedAsync(entry.Id, ct);
                    }
                    catch (Exception ex)
                    {
                        await outbox.MarkFailedAsync(entry.Id, ex.Message, ct);
                    }
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private async Task ApplyEntryAsync(SyncOutboxEntry entry, CancellationToken ct)
        {
            var url = $"{_supabaseRestUrl}/{entry.EntityTable}";
            HttpResponseMessage response;

            switch (entry.OperationType.ToUpperInvariant())
            {
                case "INSERT":
                    // UPSERT via header Prefer: resolution=merge-duplicates
                    // Si el registro ya existe en Supabase (otro dispositivo lo insert� antes),
                    // se actualiza en lugar de fallar con violaci�n de PK.
                    var insertContent = new StringContent(
                        entry.PayloadJson, Encoding.UTF8, "application/json");
                    insertContent.Headers.Add("Prefer", "resolution=merge-duplicates");

                    response = await _http.PostAsync(url, insertContent, ct);
                    break;

                case "UPDATE":
                    if (entry.EntityId is null)
                        throw new InvalidOperationException(
                            $"UPDATE sin EntityId en outbox entry {entry.Id}");

                    var updateUrl = $"{url}?id=eq.{entry.EntityId}";
                    var updateContent = new StringContent(
                        entry.PayloadJson, Encoding.UTF8, "application/json");

                    response = await _http.PatchAsync(updateUrl, updateContent, ct);
                    break;

                case "DELETE":
                    if (entry.EntityId is null)
                        throw new InvalidOperationException(
                            $"DELETE sin EntityId en outbox entry {entry.Id}");

                    response = await _http.DeleteAsync(
                        $"{url}?id=eq.{entry.EntityId}", ct);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"OperationType desconocido: {entry.OperationType}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"Supabase [{response.StatusCode}] al sincronizar {entry.OperationType} " +
                    $"en {entry.EntityTable}: {body}");
            }
        }

        public override void Dispose()
        {
            _http.Dispose();
            _syncLock.Dispose();
            base.Dispose();
        }
    }
}