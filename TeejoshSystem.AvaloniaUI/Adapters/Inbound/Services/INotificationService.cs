using System.Threading.Tasks;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public interface INotificationService
    {
        Task ShowSuccess(string message, string title = "Éxito");
        Task ShowError(string message, string title = "Error");
        Task ShowWarning(string message, string title = "Advertencia");
        Task ShowInfo(string message, string title = "Información");
    }
}