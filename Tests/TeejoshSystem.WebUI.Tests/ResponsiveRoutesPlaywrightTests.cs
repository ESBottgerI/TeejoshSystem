using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class ResponsiveRoutesPlaywrightTests
{
    private static readonly (int Width, int Height)[] Viewports =
    {
        (320, 700), (375, 760), (768, 900), (1024, 768), (1440, 900)
    };

    private static readonly string[] Routes =
    {
        "/", "/inventario", "/productos", "/productos/crear",
        "/productos/editar/1", "/productos/1", "/ventas", "/ventas/historial",
        "/cuenta/cambiar-contrasena", "/admin/usuarios",
        "/admin/catalogos/sincronizar", "/Error"
    };

    [Fact]
    public async Task AllRoutes_AreResponsiveCenteredAndDirectlyNavigable()
    {
        var baseUrl = Environment.GetEnvironmentVariable("TEEJOSH_E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 }
        });
        await LoginAsync(page, baseUrl, "admin", "admin123");

        foreach (var viewport in Viewports)
        {
            await page.SetViewportSizeAsync(viewport.Width, viewport.Height);
            foreach (var route in Routes)
            {
                await NavigateWithinCircuitAsync(page, route);
                await page.Locator("h1").First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

                var hasOverflow = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1 || document.body.scrollWidth > document.body.clientWidth + 1");
                hasOverflow.Should().BeFalse($"{route} no debe desbordar horizontalmente a {viewport.Width}px");

                var centered = await page.EvaluateAsync<bool>("() => { const header = document.querySelector('.page-header'); const main = document.querySelector('#main-content'); if (!header || !main) return true; const h = header.getBoundingClientRect(); const m = main.getBoundingClientRect(); return Math.abs((h.left + h.width / 2) - (m.left + m.width / 2)) <= 2; }");
                centered.Should().BeTrue($"el encabezado de {route} debe quedar centrado geométricamente");
            }
        }
    }

    [Fact]
    public async Task MenusRespectRole_AndSalesPanelRespondsToDesktopAndPortrait()
    {
        var baseUrl = Environment.GetEnvironmentVariable("TEEJOSH_E2E_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl)) return;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });
        await LoginAsync(page, baseUrl, "admin", "admin123");

        (await page.Locator("a[href='/productos/crear']").CountAsync()).Should().Be(1);
        (await page.Locator("a[href='/admin/usuarios']").CountAsync()).Should().Be(1);

        await NavigateWithinCircuitAsync(page, "/ventas");
        await page.Locator(".sales-split").WaitForAsync();
        (await page.Locator(".sales-separator").EvaluateAsync<string>("element => getComputedStyle(element).display"))
            .Should().NotBe("none");
        var columns = await page.Locator(".sales-split").EvaluateAsync<string>("element => getComputedStyle(element).gridTemplateColumns");
        columns.Split(' ', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(3);

        await page.SetViewportSizeAsync(768, 1100);
        (await page.Locator(".sales-separator").EvaluateAsync<string>("element => getComputedStyle(element).display"))
            .Should().Be("none");
        var portraitColumns = await page.Locator(".sales-split").EvaluateAsync<string>("element => getComputedStyle(element).gridTemplateColumns");
        portraitColumns.Split(' ', StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(1);
    }

    private static async Task LoginAsync(IPage page, string baseUrl, string username, string password)
    {
        await page.GotoAsync(baseUrl.TrimEnd('/') + "/login", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(1500);
        await page.Locator("input:not([type=password])").FillAsync(username);
        await page.Locator("input[type=password]").FillAsync(password);
        await page.Locator("button[type=submit]").ClickAsync();
        await page.WaitForFunctionAsync("() => location.pathname !== '/login'");
        await page.Locator("a[href='/admin/usuarios']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    private static async Task NavigateWithinCircuitAsync(IPage page, string route)
    {
        await page.EvaluateAsync("route => Blazor.navigateTo(route)", route);
        await page.WaitForFunctionAsync("route => location.pathname.toLowerCase() === route.toLowerCase()", route);
    }
}