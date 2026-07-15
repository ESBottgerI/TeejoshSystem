using System.Threading;
using System.Threading.Tasks;
using System;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public class NavigationService : INavigationService
{
    private Action<object>? _navigate;
    private Action? _navigateToMenu;

    public void Configure(Action<object> navigate, Action navigateToMenu)
    {
        _navigate = navigate;
        _navigateToMenu = navigateToMenu;
    }

    public async Task NavigateToAsync(object viewModel, CancellationToken cancellationToken = default)
    {
        if (viewModel is ILoadable loadable)
            await loadable.LoadAsync(cancellationToken).ConfigureAwait(true);

        _navigate?.Invoke(viewModel);
    }

    public Task NavigateToMenuAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _navigateToMenu?.Invoke();
        return Task.CompletedTask;
    }
}