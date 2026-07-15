using System.Threading;
using System.Threading.Tasks;
namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public interface INavigationService
{
    Task NavigateToAsync(object viewModel, CancellationToken cancellationToken = default);
    Task NavigateToMenuAsync(CancellationToken cancellationToken = default);
}
