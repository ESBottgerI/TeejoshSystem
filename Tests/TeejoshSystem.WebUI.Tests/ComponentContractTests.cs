using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.WebUI.Components.Layout;
using TeejoshSystem.WebUI.Components.Shared.Common;
using TeejoshSystem.WebUI.Infrastructure.Services;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class ComponentContractTests
{
    [Fact]
    public void PageHeader_RendersSemanticHeadingAndActions()
    {
        using var context = new TestContext();
        var cut = context.RenderComponent<PageHeader>(parameters => parameters
            .Add(p => p.Title, "Inventario")
            .Add(p => p.Eyebrow, "Stock general")
            .Add(p => p.Actions, builder => builder.AddMarkupContent(0, "<button>Nuevo</button>")));

        cut.Find("header.page-header").Should().NotBeNull();
        cut.Find("h1").TextContent.Should().Be("Inventario");
        cut.Find(".eyebrow").TextContent.Should().Be("Stock general");
        cut.Find(".page-actions button").TextContent.Should().Be("Nuevo");
    }

    [Theory]
    [InlineData("Operador", false)]
    [InlineData("Administrador", true)]
    public void NavMenu_ExposesAdministrativeRoutesOnlyToAdministrators(string role, bool expected)
    {
        using var context = new TestContext();
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized("usuario");
        auth.SetRoles(role);

        var markup = context.RenderComponent<NavMenu>().Markup;

        markup.Should().Contain("href=\"/inventario\"");
        markup.Should().Contain("href=\"/ventas\"");
        markup.Contains("href=\"/productos/crear\"").Should().Be(expected);
        markup.Contains("href=\"/admin/usuarios\"").Should().Be(expected);
        markup.Contains("href=\"/admin/catalogos/sincronizar\"").Should().Be(expected);
    }

    [Fact]
    public async Task ConfirmDialog_CompletesPendingRequest()
    {
        using var context = new TestContext();
        var service = new BlazorConfirmationService();
        context.Services.AddSingleton(service);
        var cut = context.RenderComponent<TeejoshSystem.WebUI.Components.Shared.Feedback.ConfirmDialog>();

        var pending = service.ConfirmAsync("Eliminar producto", "Confirmar");
        cut.WaitForElement("[role=alertdialog]");
        cut.FindAll("button").Single(button => button.TextContent == "Confirmar").Click();

        (await pending).Should().BeTrue();
        cut.Markup.Should().NotContain("alertdialog");
    }

    [Fact]
    public void EveryDefinitiveRouteIsDiscoveredFromComponentMetadata()
    {
        var discovered = typeof(TeejoshSystem.WebUI.Components.Pages.Ventas.VentasPage)
            .Assembly.ExportedTypes
            .SelectMany(type => type.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                .Cast<RouteAttribute>()
                .Select(route => route.Template))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expected = new[]
        {
            "/", "/login", "/inventario", "/productos", "/productos/crear",
            "/productos/editar/{Id:int}", "/productos/{Id:int}", "/ventas",
            "/ventas/historial", "/cuenta/cambiar-contrasena", "/admin/usuarios",
            "/admin/catalogos/sincronizar", "/Error"
        };

        discovered.Should().Contain(expected);
    }
}

