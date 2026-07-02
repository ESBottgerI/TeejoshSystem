namespace TeejoshSystem.WebUI.Infrastructure.Services;

public sealed class BlazorConfirmationService
{
    private TaskCompletionSource<bool>? _pendingTcs;

    public Task<bool> ConfirmAsync(string message, string title = "Confirmacion")
    {
        _pendingTcs = new TaskCompletionSource<bool>();
        OnConfirmRequested?.Invoke(new ConfirmDialog(message, title, SetResult));
        return _pendingTcs.Task;
    }

    public event Action<ConfirmDialog>? OnConfirmRequested;

    private void SetResult(bool result)
    {
        _pendingTcs?.TrySetResult(result);
        _pendingTcs = null;
    }
}

public sealed record ConfirmDialog(string Message, string Title, Action<bool> Callback);
