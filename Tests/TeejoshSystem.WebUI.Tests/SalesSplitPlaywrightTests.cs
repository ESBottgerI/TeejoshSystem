using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class SalesSplitPlaywrightTests
{
    [Fact]
    public async Task Split_ClampsHonorsBreakpointAndPortrait_AndDisposesListeners()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1200, Height = 700 } });
        await page.SetContentAsync("<div id='root' style='position:relative;width:1000px;height:300px'><button id='separator' style='position:absolute;left:500px'>split</button></div>");
        var script = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "TeejoshSystem.WebUI", "wwwroot", "js", "sales-split.js"));
        await page.AddScriptTagAsync(new() { Path = script });
        await page.EvaluateAsync("window.handle = window.teejoshSalesSplit.attach(document.querySelector('#root'), document.querySelector('#separator'))");

        await DragAsync(page, 500, 10);

        (await SalesLeft(page)).Should().Be("352px");

        await page.EvaluateAsync("window.handle.dispose(); window.handle = window.teejoshSalesSplit.attach(document.querySelector('#root'), document.querySelector('#separator'))");
        await DragAsync(page, 500, 999);

        (await SalesLeft(page)).Should().Be("680px");

        await page.SetViewportSizeAsync(768, 700);
        await DragAsync(page, 500, 400);
        (await SalesLeft(page)).Should().Be("680px");

        await page.SetViewportSizeAsync(1024, 1200);
        await DragAsync(page, 500, 400);
        (await SalesLeft(page)).Should().Be("680px");

        await page.SetViewportSizeAsync(1200, 700);
        await page.EvaluateAsync("window.handle.dispose()");
        await DragAsync(page, 500, 450);
        (await SalesLeft(page)).Should().Be("680px");
    }

    private static async Task DragAsync(IPage page, float from, float to)
    {
        var fromText = from.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var toText = to.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await page.EvaluateAsync($"() => {{ const separator = document.querySelector('#separator'); separator.dispatchEvent(new MouseEvent('mousedown', {{ button: 0, clientX: {fromText}, bubbles: true }})); document.dispatchEvent(new MouseEvent('mousemove', {{ clientX: {toText}, bubbles: true }})); document.dispatchEvent(new MouseEvent('mouseup', {{ clientX: {toText}, bubbles: true }})); }}");
    }

    private static Task<string> SalesLeft(IPage page) =>
        page.EvalOnSelectorAsync<string>("#root", "element => element.style.getPropertyValue('--sales-left')");
}





