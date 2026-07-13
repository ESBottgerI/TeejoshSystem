using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public class SessionCurrentUserProvider : ICurrentUserProvider
    {
        private readonly SesionContext _sesionContext;

        public SessionCurrentUserProvider(SesionContext sesionContext)
        {
            _sesionContext = sesionContext;
        }

        public string? UsuarioActual => _sesionContext.SesionActual?.NombreUsuario;
    }
}