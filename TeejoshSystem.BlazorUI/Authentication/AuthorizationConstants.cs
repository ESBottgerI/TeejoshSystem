using TeejoshSystem.Domain.Enums;

namespace TeejoshSystem.BlazorUI.Authentication;

/// <summary>
/// Centraliza el nombre del rol usado en [Authorize(Roles = ...)].
/// Usa nameof() sobre el enum real en vez de un string mágico repetido en
/// cada página — si el rol se renombra en Domain, esto no compila hasta que
/// se actualice, en vez de fallar en runtime en silencio.
/// </summary>
public static class AuthorizationConstants
{
    public const string Administrador = nameof(RolUsuario.Administrador);
}