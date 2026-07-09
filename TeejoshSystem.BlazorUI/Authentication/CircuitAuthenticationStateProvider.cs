using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TeejoshSystem.BlazorUI.Authentication;

/// <summary>
/// Traduce BlazorSessionContext (estado de sesión del circuito) a un
/// ClaimsPrincipal que el resto de Blazor entiende ([Authorize], AuthorizeView,
/// context.User, etc.).
///
/// Deliberadamente NO usa JWT ni cookies: Blazor Server ya mantiene un canal
/// persistente con estado (el circuito SignalR), así que la sesión vive ahí,
/// igual de "server-side" en espíritu que el SesionContext de Avalonia.
/// JWT tendría sentido en una API stateless — no es el caso aquí.
/// </summary>
public class CircuitAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly BlazorSessionContext _sessionContext;

    public CircuitAuthenticationStateProvider(BlazorSessionContext sessionContext)
    {
        _sessionContext = sessionContext;

        // Cuando Login/Logout cambian la sesión, se recalcula el estado y se
        // notifica a la UI (así AuthorizeView/[Authorize] reaccionan sin
        // necesidad de recargar la página).
        _sessionContext.SesionCambiada += NotificarCambioDeSesion;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var sesion = _sessionContext.SesionActual;

        var identity = sesion is null
            ? new ClaimsIdentity() // anónimo, sin authenticationType
            : new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, sesion.NombreUsuario),
                    new Claim(ClaimTypes.Role, sesion.Rol.ToString())
                },
                authenticationType: "TeejoshCircuitSession");

        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(principal));
    }

    private void NotificarCambioDeSesion()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}