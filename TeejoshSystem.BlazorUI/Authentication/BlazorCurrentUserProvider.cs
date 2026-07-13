using Microsoft.AspNetCore.Components.Authorization;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.BlazorUI.Authentication;

/// <summary>
/// Reemplaza a la versión anterior basada en BlazorSessionContext (eliminada).
/// Ahora la sesión vive en la cookie de autenticación de ASP.NET Core, así
/// que el usuario actual se lee directamente del ClaimsPrincipal expuesto
/// por AuthenticationStateProvider — funciona igual durante una request
/// estática (SSR) que dentro de un circuito interactivo ya establecido.
///
/// Nota de diseño: ICurrentUserProvider (definido en Domain) expone
/// UsuarioActual de forma síncrona, pero GetAuthenticationStateAsync() es
/// async. El bloqueo con GetAwaiter().GetResult() es aceptable aquí porque
/// es una operación en memoria (sin I/O real) — no golpea la DB ni la red.
/// </summary>
public class BlazorCurrentUserProvider : ICurrentUserProvider
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorCurrentUserProvider(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public string? UsuarioActual =>
        _authStateProvider.GetAuthenticationStateAsync()
            .GetAwaiter()
            .GetResult()
            .User.Identity?.Name;
}