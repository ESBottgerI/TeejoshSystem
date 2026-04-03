

namespace TeejoshInventario.WPF.Adapters.Inbound.Services
{
    public interface INotificationService
    {
        void ShowSuccess(string message);
        void ShowError(string message);
    }
}
