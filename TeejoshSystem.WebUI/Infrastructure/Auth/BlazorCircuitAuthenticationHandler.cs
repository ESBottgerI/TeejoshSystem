using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TeejoshSystem.WebUI.Infrastructure.Auth;

public sealed class BlazorCircuitAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "BlazorCircuit";

    public BlazorCircuitAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(AuthenticateResult.NoResult());

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var returnUrl = Request.PathBase.Add(Request.Path).ToString();
        if (Request.QueryString.HasValue)
        {
            returnUrl += Request.QueryString.Value;
        }

        Response.Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl.TrimStart('/'))}");
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect("/");
        return Task.CompletedTask;
    }
}
