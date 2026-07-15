using FluentAssertions;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Storage;
using Xunit;

namespace TeejoshSystem.Infrastructure.Tests.Storage;

public sealed class LocalImageStorageSecurityTests
{
    [Theory]
    [InlineData("../secret.png")]
    [InlineData("..\\secret.png")]
    [InlineData("folder/image.png")]
    [InlineData("C:\\temp\\image.png")]
    public async Task ReadImage_RejectsPathsOutsideStorageRoot(string path)
    {
        var folder = Path.Combine(Path.GetTempPath(), $"teejosh-images-{Guid.NewGuid():N}");
        try
        {
            var service = new LocalImageStorageService(folder);
            (await service.ReadImageAsync(path, thumbnail: false)).Should().BeNull();
            service.GetFullPath(path).Should().BeNull();
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task SaveImage_RejectsExtensionSpoofingAndInvalidBytes()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"teejosh-images-{Guid.NewGuid():N}");
        var source = Path.Combine(Path.GetTempPath(), $"teejosh-spoof-{Guid.NewGuid():N}.png");
        try
        {
            await File.WriteAllTextAsync(source, "esto no es una imagen");
            var service = new LocalImageStorageService(folder);
            (await service.SaveImageAsync(source)).Should().BeNull();
            Directory.EnumerateFiles(folder).Should().BeEmpty();
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }
}