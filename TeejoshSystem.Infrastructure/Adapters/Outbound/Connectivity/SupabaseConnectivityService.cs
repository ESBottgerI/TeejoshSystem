using Microsoft.Extensions.Hosting;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Connectivity
{
    /// <summary>
    /// Implementación de IConnectivityService que corre como BackgroundService.
    /// Hace ping HTTP al endpoint de Supabase cada <see cref="PingIntervalSeconds"/> segundos.
    ///
    /// Estrategia de detección:
    ///   1. GET https://{ref}.supabase.co/rest/v1/ con apikey header
    ///      → si responde (cualquier código HTTP) = Supabase es alcanzable = ONLINE
    ///      → si lanza excepción de red (timeout, DNS, refused) = OFFLINE
    ///
    /// Esto detecta tanto fallo de red local como caída del servicio Supabase.
    /// </summary>
    public class SupabaseConnectivityService : BackgroundService, IConnectivityService
    {
        private readonly HttpClient _http;
        private readonly string _pingUrl;
        private readonly int _pingIntervalSeconds;

        private bool _isOnline = false;
        private bool _firstCheck = true;

        public bool IsOnline => _isOnline;
        public event Action<bool>? ConnectivityChanged;

        public SupabaseConnectivityService(
            string supabaseUrl,
            string supabaseAnonKey,
            int pingIntervalSeconds = 15)
        {
            _pingIntervalSeconds = pingIntervalSeconds;

            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            _http.DefaultRequestHeaders.Add("apikey", supabaseAnonKey);

            // El endpoint REST raíz de Supabase responde 200 con el schema OpenAPI
            _pingUrl = $"{supabaseUrl.TrimEnd('/')}/rest/v1/";
        }

        public async Task<bool> CheckNowAsync(CancellationToken ct = default)
        {
            var online = await PingAsync(ct);
            UpdateState(online);
            return online;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Primera verificación inmediata al arrancar la app
            await CheckNowAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_pingIntervalSeconds), stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                var online = await PingAsync(stoppingToken);
                UpdateState(online);
            }
        }

        private async Task<bool> PingAsync(CancellationToken ct)
        {
            try
            {
                var response = await _http.GetAsync(_pingUrl, ct);
                // Cualquier respuesta HTTP (200, 400, 401) significa que el servidor está vivo.
                // Solo una excepción de red indica OFFLINE.
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateState(bool newState)
        {
            // En el primer check no comparamos — simplemente establecemos el estado inicial.
            if (_firstCheck)
            {
                _firstCheck = false;
                _isOnline = newState;
                ConnectivityChanged?.Invoke(newState);
                return;
            }

            if (newState == _isOnline) return;

            _isOnline = newState;
            ConnectivityChanged?.Invoke(newState);
        }

        public override void Dispose()
        {
            _http.Dispose();
            base.Dispose();
        }
    }
}