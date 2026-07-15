using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Common.Dtos;
using TeejoshSystem.Application.Ports.Inbound.Auth.Commands.AutenticarUsuario;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerProductos;
using TeejoshSystem.Domain.Enums;
using TeejoshSystem.WebUI.Components;
using TeejoshSystem.WebUI.Components.Account.Pages;
using TeejoshSystem.WebUI.Components.Layout;
using TeejoshSystem.WebUI.Components.Pages.Inventario;
using TeejoshSystem.WebUI.Components.Shared.Common;
using TeejoshSystem.WebUI.Extensions;
using TeejoshSystem.WebUI.Infrastructure.Auth;
using TeejoshSystem.WebUI.Infrastructure.Services;
using TeejoshSystem.WebUI.Infrastructure.State;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class WebUiBehaviorTests
{
    [Fact]
    public void MainLayout_RendersOwnShellAndBody()
    {
        using var context = CreateContext(out _, out _);
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized("admin");
        auth.SetRoles("Administrador");

        var cut = context.RenderComponent<MainLayout>(parameters => parameters
            .Add(layout => layout.Body, builder => builder.AddMarkupContent(0, "<h1>Contenido</h1>")));

        cut.Find(".app-shell").Should().NotBeNull();
        cut.Find("aside.side-nav").Should().NotBeNull();
        cut.Find("main#main-content h1").TextContent.Should().Be("Contenido");
    }

    [Fact]
    public void LoadingOverlay_RendersOnlyWhileVisible()
    {
        using var context = new TestContext();
        var cut = context.RenderComponent<LoadingOverlay>(p => p
            .Add(x => x.IsVisible, false)
            .Add(x => x.Message, "Consultando"));
        cut.Markup.Should().NotContain("loading-overlay");

        cut.SetParametersAndRender(p => p.Add(x => x.IsVisible, true));
        cut.Find("[role=status]").TextContent.Should().Contain("Consultando");
    }

    [Fact]
    public void Inventario_ShowsSuccessfulResultAndClearsBusy()
    {
        using var context = CreateContext(out var mediator, out _);
        mediator.Send(Arg.Any<ObtenerProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductoDto>
            {
                new() { Id = 7, Nombre = "Mustang", Precio = 25m, Unidades = 2,
                    Tipo = TipoProducto.HotWheels, TipoDescripcion = "Hot Wheels",
                    DetalleResumen = "Serie" }
            });

        var cut = context.RenderComponent<InventarioPage>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Mustang"));
        cut.Markup.Should().NotContain("loading-overlay");
    }
    [Fact]
    public void Inventario_ShowsControlledErrorAndClearsBusy()
    {
        using var context = CreateContext(out var mediator, out _);
        mediator.Send(Arg.Any<ObtenerProductosQuery>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<IReadOnlyList<ProductoDto>>(new InvalidOperationException("offline")));

        var cut = context.RenderComponent<InventarioPage>();
        cut.WaitForAssertion(() => cut.Find("[role=alert]").TextContent.Should().Contain("offline"));
        cut.Markup.Should().NotContain("loading-overlay");
    }

    [Fact]
    public async Task Login_UsesRealAuthenticationCommandAndCreatesSession()
    {
        using var context = CreateContext(out var mediator, out var session);
        var expected = new SesionDto(1, "admin", RolUsuario.Administrador);
        mediator.Send(Arg.Any<AutenticarUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expected));
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/login?returnUrl=%2Finventario");

        var cut = context.RenderComponent<LoginPage>();
        cut.Find("input:not([type=password])").Input("admin");
        cut.Find("input[type=password]").Input("secreto");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => session.EstaAutenticado.Should().BeTrue());
        context.Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/inventario");
        await mediator.Received(1).Send(
            Arg.Is<AutenticarUsuarioCommand>(command => command.NombreUsuario == "admin"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Login_DisplaysCredentialFailureWithoutCreatingSession()
    {
        using var context = CreateContext(out var mediator, out var session);
        mediator.Send(Arg.Any<AutenticarUsuarioCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SesionDto>("Credenciales inválidas"));

        var cut = context.RenderComponent<LoginPage>();
        cut.Find("input:not([type=password])").Input("operador");
        cut.Find("input[type=password]").Input("incorrecta");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.Find("[role=alert]").TextContent.Should().Contain("Credenciales inválidas"));
        session.EstaAutenticado.Should().BeFalse();
    }

    [Fact]
    public void Logout_ClearsSessionAndCartAndNavigatesToLogin()
    {
        using var context = CreateContext(out _, out var session);
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized("operador");
        auth.SetRoles("Operador");
        session.IniciarSesion(new SesionDto(2, "operador", RolUsuario.Operador));
        var cart = context.Services.GetRequiredService<CircuitStateStore>();
        cart.AddOrIncrement(5, "Producto", 10m, 2);

        var cut = context.RenderComponent<UserProfileHeader>();
        cut.Find("button").Click();

        session.EstaAutenticado.Should().BeFalse();
        cart.CartItems.Should().BeEmpty();
        context.Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/login");
    }

    [Fact]
    public void AnonymousDirectAccess_RedirectsToLoginWithReturnUrl()
    {
        using var context = CreateContext(out _, out _);
        var auth = context.AddTestAuthorization();
        auth.SetNotAuthorized();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/inventario");

        var cut = context.RenderComponent<Routes>();

        cut.WaitForAssertion(() => navigation.Uri.Should().Contain("/login?returnUrl=inventario"));
    }

    [Fact]
    public void OperatorDirectAccess_ToAdminRoute_ShowsAccessDenied()
    {
        using var context = CreateContext(out _, out _);
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized("operador");
        auth.SetRoles("Operador");
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/productos");

        var cut = context.RenderComponent<Routes>();

        cut.WaitForAssertion(() => cut.Find("[role=alert] h1").TextContent.Should().Be("Acceso denegado"));
    }

    [Fact]
    public void TwoDependencyInjectionScopes_IsolateSessionCartAndAuthenticationProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IMediator>());
        services.AddTeejoshWebUi();
        using var provider = services.BuildServiceProvider();
        using var first = provider.CreateScope();
        using var second = provider.CreateScope();

        var firstSession = first.ServiceProvider.GetRequiredService<BlazorSesionContext>();
        var secondSession = second.ServiceProvider.GetRequiredService<BlazorSesionContext>();
        var firstCart = first.ServiceProvider.GetRequiredService<CircuitStateStore>();
        var secondCart = second.ServiceProvider.GetRequiredService<CircuitStateStore>();
        firstSession.IniciarSesion(new SesionDto(1, "admin", RolUsuario.Administrador));
        firstCart.AddOrIncrement(1, "Producto", 12m, 4);

        secondSession.EstaAutenticado.Should().BeFalse();
        secondCart.CartItems.Should().BeEmpty();
        first.ServiceProvider.GetRequiredService<CustomAuthenticationStateProvider>()
            .Should().NotBeSameAs(second.ServiceProvider.GetRequiredService<CustomAuthenticationStateProvider>());
    }

    private static TestContext CreateContext(out IMediator mediator, out BlazorSesionContext session)
    {
        var context = new TestContext();
        mediator = Substitute.For<IMediator>();
        session = new BlazorSesionContext();
        var provider = new CustomAuthenticationStateProvider(session, mediator);
        context.Services.AddSingleton(mediator);
        context.Services.AddSingleton(session);
        context.Services.AddSingleton(provider);
        context.Services.AddSingleton<CircuitStateStore>();
        context.Services.AddSingleton<BlazorNotificationService>();
        context.Services.AddSingleton<BlazorConfirmationService>();
        return context;
    }
}