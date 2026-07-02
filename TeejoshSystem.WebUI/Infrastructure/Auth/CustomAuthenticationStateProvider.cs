using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;

namespace TeejoshSystem.WebUI.Infrastructure.Auth;

public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private readonly BlazorSesionContext _sesionContext;
    private readonly IMediator _mediator;

    public CustomAuthenticationStateProvider(
        BlazorSesionContext sesionContext,
        IMediator mediator)
    {
        _sesionContext = sesionContext;
        _mediator = mediator;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_sesionContext.EstaAutenticado || _sesionContext.SesionActual is null)
            return Task.FromResult(new AuthenticationState(Anonymous));

        var principal = BuildPrincipal(_sesionContext.SesionActual);
        return Task.FromResult(new AuthenticationState(principal));
    }

    public async Task<(bool Success, string? Error)> SignInWithPasswordAsync(
        string nombreUsuario, string password)
    {
        var result = await _mediator.Send(
            new AutenticarUsuarioCommand(nombreUsuario, password));

        if (!result.IsSuccess)
            return (false, result.Error);

        _sesionContext.IniciarSesion(result.Value);
        var principal = BuildPrincipal(result.Value);
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal)));
        return (true, null);
    }

    public Task SignOutAsync()
    {
        _sesionContext.CerrarSesion();
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(Anonymous)));
        return Task.CompletedTask;
    }

    private static ClaimsPrincipal BuildPrincipal(SesionDto sesion)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, sesion.NombreUsuario),
            new(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
            new(ClaimTypes.Role, sesion.Rol.ToString())
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "LocalAuth"));
    }
}
