using System;
using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    /// <summary>
    /// Sesión activa en memoria. Singleton — una instancia en toda la app.
    /// No persiste entre reinicios: login obligatorio en cada arranque.
    /// Solo la UI lee y escribe este contexto.
    /// </summary>
    public class SesionContext
    {
        private SesionDto? _sesionActual;

        public bool EstaAutenticado => _sesionActual is not null;
        public SesionDto? SesionActual => _sesionActual;

        public void IniciarSesion(SesionDto sesion)
        {
            ArgumentNullException.ThrowIfNull(sesion);
            _sesionActual = sesion;
        }

        public void CerrarSesion() => _sesionActual = null;
    }
}