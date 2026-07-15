using System.Threading;
using System.Threading.Tasks;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public interface IImagePreviewService
{
    Task ShowAsync(byte[]? image, string productName, CancellationToken cancellationToken = default);
}