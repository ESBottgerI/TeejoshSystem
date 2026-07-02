namespace TeejoshSystem.WebUI.Infrastructure.Services;

public sealed class BlazorNotificationService
{
    public event Action<ToastMessage>? OnNotify;

    public Task ShowSuccessAsync(string message) => ShowAsync("success", message);

    public Task ShowErrorAsync(string message) => ShowAsync("error", message);

    public Task ShowInfoAsync(string message) => ShowAsync("info", message);

    private Task ShowAsync(string kind, string message)
    {
        OnNotify?.Invoke(new ToastMessage(kind, message));
        return Task.CompletedTask;
    }
}

public sealed record ToastMessage(string Kind, string Message);
