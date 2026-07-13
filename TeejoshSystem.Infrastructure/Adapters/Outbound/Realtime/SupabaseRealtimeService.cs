using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Realtime
{
    /// <summary>
    /// Implementación de IRealtimeService usando WebSocket raw con protocolo Phoenix Channels.
    /// No usa el SDK oficial de Supabase para C# (conflictos con System.Reactive en Avalonia).
    ///
    /// Protocolo Phoenix Channels resumido:
    ///   1. Conectar WebSocket a wss://{ref}.supabase.co/realtime/v1/websocket?apikey={key}
    ///   2. Enviar heartbeat cada 30s: {"topic":"phoenix","event":"heartbeat","payload":{},"ref":"0"}
    ///   3. Join al channel "realtime:public:{table}":
    ///      {"topic":"realtime:public:{table}","event":"phx_join","payload":{"config":{"broadcast":{"self":false}}},"ref":"1"}
    ///   4. Recibir eventos con event = "INSERT" | "UPDATE" | "DELETE"
    ///
    /// Esta implementación se activa SOLO cuando Provider = "postgresql".
    /// En modo sqlite no se instancia (no hay Supabase a qué conectarse).
    /// </summary>
    public class SupabaseRealtimeService : BackgroundService, IRealtimeService
    {
        private readonly string _wsUrl;
        private readonly string _apiKey;

        private ClientWebSocket? _ws;
        private readonly List<(string Table, Action<RealtimeEvent> Callback)> _subscriptions = new();
        private readonly SemaphoreSlim _wsLock = new(1, 1);
        private int _refCounter = 1;

        public bool IsConnected =>
            _ws?.State == WebSocketState.Open;

        public SupabaseRealtimeService(string supabaseUrl, string supabaseAnonKey)
        {
            _apiKey = supabaseAnonKey;
            // Convertir https:// → wss://
            var wsBase = supabaseUrl
                .TrimEnd('/')
                .Replace("https://", "wss://")
                .Replace("http://", "ws://");

            _wsUrl = $"{wsBase}/realtime/v1/websocket?apikey={supabaseAnonKey}&vsn=1.0.0";
        }

        public async Task SubscribeAsync(
            string table,
            Action<RealtimeEvent> onEvent,
            CancellationToken ct = default)
        {
            _subscriptions.Add((table, onEvent));

            // Si ya está conectado, enviar el join inmediatamente
            if (IsConnected)
                await JoinChannelAsync(table, ct);
        }

        public async Task UnsubscribeAllAsync()
        {
            _subscriptions.Clear();
            if (_ws is not null && _ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "UnsubscribeAll",
                    CancellationToken.None);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndListenAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch
                {
                    // Reconectar tras 10s en caso de fallo
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task ConnectAndListenAsync(CancellationToken ct)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(_wsUrl), ct);

            // Unirse a todos los canales ya registrados
            foreach (var (table, _) in _subscriptions)
                await JoinChannelAsync(table, ct);

            // Heartbeat cada 30s en background
            _ = Task.Run(() => HeartbeatLoopAsync(ct), ct);

            // Loop de recepción de mensajes
            var buffer = new byte[8192];
            while (_ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _ws.ReceiveAsync(buffer, ct);
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(ms.ToArray());
                    DispatchMessage(json);
                }
            }
        }

        private async Task JoinChannelAsync(string table, CancellationToken ct)
        {
            var topic = $"realtime:public:{table}";
            var joinMsg = JsonSerializer.Serialize(new
            {
                topic,
                @event = "phx_join",
                payload = new
                {
                    config = new
                    {
                        broadcast = new { self = false },
                        postgres_changes = new[]
                        {
                            new { @event = "*", schema = "public", table }
                        }
                    }
                },
                @ref = (_refCounter++).ToString()
            });

            await SendAsync(joinMsg, ct);
        }

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
                if (_ws?.State != WebSocketState.Open) break;

                var heartbeat = JsonSerializer.Serialize(new
                {
                    topic = "phoenix",
                    @event = "heartbeat",
                    payload = new { },
                    @ref = "0"
                });

                await SendAsync(heartbeat, ct);
            }
        }

        private void DispatchMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("event", out var eventProp)) return;
                var eventType = eventProp.GetString();

                // Supabase Realtime v2 empaqueta los cambios en payload.data
                if (eventType != "INSERT" && eventType != "UPDATE" && eventType != "DELETE")
                    return;

                if (!root.TryGetProperty("topic", out var topicProp)) return;
                var topic = topicProp.GetString() ?? "";

                // topic = "realtime:public:{table}"
                var parts = topic.Split(':');
                if (parts.Length < 3) return;
                var table = parts[2];

                if (!root.TryGetProperty("payload", out var payload)) return;

                // El payload puede venir en "record" (v1) o en "data.record" (v2)
                string payloadJson;
                if (payload.TryGetProperty("record", out var record))
                    payloadJson = record.GetRawText();
                else if (payload.TryGetProperty("data", out var data) &&
                         data.TryGetProperty("record", out var dataRecord))
                    payloadJson = dataRecord.GetRawText();
                else
                    payloadJson = payload.GetRawText();

                var evt = new RealtimeEvent(eventType!, table, "public", payloadJson);

                foreach (var (subTable, callback) in _subscriptions)
                {
                    if (subTable.Equals(table, StringComparison.OrdinalIgnoreCase))
                        callback(evt);
                }
            }
            catch
            {
                // Mensaje malformado — ignorar silenciosamente
            }
        }

        private async Task SendAsync(string message, CancellationToken ct)
        {
            await _wsLock.WaitAsync(ct);
            try
            {
                if (_ws?.State != WebSocketState.Open) return;
                var bytes = Encoding.UTF8.GetBytes(message);
                await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
            }
            finally
            {
                _wsLock.Release();
            }
        }

        public override void Dispose()
        {
            _ws?.Dispose();
            _wsLock.Dispose();
            base.Dispose();
        }
    }
}