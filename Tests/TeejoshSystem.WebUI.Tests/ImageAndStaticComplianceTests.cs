using Bunit;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using TeejoshSystem.Application.Common;
using TeejoshSystem.Application.Ports.Inbound.Productos.Queries.ObtenerImagenProducto;
using TeejoshSystem.WebUI.Components.Shared.Display;
using Xunit;

namespace TeejoshSystem.WebUI.Tests;

public sealed class ImageAndStaticComplianceTests
{
    [Fact]
    public void ProductImage_UsesBlobUrlFocusesModalAndRevokesUrls()
    {
        using var context = new TestContext();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ObtenerImagenProductoQuery>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var query = call.Arg<ObtenerImagenProductoQuery>();
                var content = query.Variante == VarianteImagen.Thumbnail ? new byte[] { 1 } : new byte[] { 2 };
                return Result.Success(new ProductoImagenDto(content, "image/png"));
            });
        context.Services.AddSingleton(mediator);
        context.JSInterop.Setup<string>("teejoshInterop.createObjectUrl", _ => true)
            .SetResult("blob:image");
        context.JSInterop.SetupVoid("teejoshInterop.focus", _ => true);
        context.JSInterop.SetupVoid("teejoshInterop.revokeObjectUrl", _ => true);

        var cut = context.RenderComponent<ProductIdentityCell>(parameters => parameters
            .Add(x => x.ProductId, 7)
            .Add(x => x.Name, "Producto")
            .Add(x => x.HasImage, true));
        cut.WaitForAssertion(() => cut.Find("img").GetAttribute("src").Should().StartWith("blob:"));

        cut.Find("button.product-thumbnail-button").Click();
        cut.WaitForElement("[role=dialog]");
        cut.WaitForAssertion(() => context.JSInterop.Invocations
            .Should().Contain(invocation => invocation.Identifier == "teejoshInterop.focus"));
        cut.Find("button.image-modal-close").Click();
        cut.WaitForAssertion(() => context.JSInterop.Invocations
            .Should().Contain(invocation => invocation.Identifier == "teejoshInterop.revokeObjectUrl"));
    }

    [Fact]
    public void AvaloniaFunctionalCode_HasNoExplicitFireAndForgetPatterns()
    {
        var root = RepositoryRoot();
        var files = Directory.EnumerateFiles(Path.Combine(root, "TeejoshSystem.AvaloniaUI"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
        var source = string.Join("\n", files.Select(File.ReadAllText));

        source.Should().NotContain("_ = ");
        source.Should().NotContain("async void");
        source.Should().NotContain("Task.Run(");
        source.Should().NotContain("ContinueWith(");
    }

    [Fact]
    public void ProductionWebUi_DisablesAutomaticMigrationsAndBlobInteropRevokes()
    {
        var root = RepositoryRoot();
        var production = File.ReadAllText(Path.Combine(root, "TeejoshSystem.WebUI", "appsettings.Production.json"));
        production.Should().Contain("\"ApplyMigrationsOnStartup\": false");
        var program = File.ReadAllText(Path.Combine(root, "TeejoshSystem.WebUI", "Program.cs"));
        program.Should().NotContain("Database.Migrate");
        var interop = File.ReadAllText(Path.Combine(root, "TeejoshSystem.WebUI", "wwwroot", "js", "blazor-interop.js"));
        interop.Should().Contain("URL.createObjectURL");
        interop.Should().Contain("URL.revokeObjectURL");
    }

    private static string RepositoryRoot() => Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
}