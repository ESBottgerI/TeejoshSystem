using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace TeejoshSystem.WebUI.Infrastructure.Auth;

public sealed class BlazorUserContext
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public BlazorUserContext(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<string> GetUserNameAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.Name) ?? "web";
    }
}
