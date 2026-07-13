using TeejoshSystem.Application.Common.Dtos;

namespace TeejoshSystem.BlazorUI.Authentication;

/// <summary>
/// Sesión activa en memoria — equivalente funcional de
/// TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services.SesionContext.
///
/// Diferencia crítica respecto a Avalonia: allá es Singleton porque hay un
/// solo usuario por proceso de escritorio. Aquí DEBE registrarse Scoped,
/// porque en Blazor Server cada usuario conectado tiene su propio circuito
/// (su propio scope de DI). Si esto se registrara Singleton, la sesión de
/// un administrador se filtraría a todos los demás usuarios conectados al
/// mismo servidor — es el error más fácil de cometer copiando Avalonia
/// sin ajustar el lifetime. Ver registro en ServiceCollectionExtensions.
/// </summary>
public class BlazorSessionContext
{
    private SesionDto? _sesionActual;

    public bool EstaAutenticado => _sesionActual is not null;
    public SesionDto? SesionActual => _sesionActual;

    /// <summary>
    /// Notifica a CircuitAuthenticationStateProvider que debe recalcular
    /// el ClaimsPrincipal y refrescar la UI (AuthorizeView, [Authorize], etc.).
    /// </summary>
    public event Action? SesionCambiada;

    public void IniciarSesion(SesionDto sesion)
    {
        ArgumentNullException.ThrowIfNull(sesion);
        _sesionActual = sesion;
        SesionCambiada?.Invoke();
    }

    public void CerrarSesion()
    {
        _sesionActual = null;
        SesionCambiada?.Invoke();
    }
}