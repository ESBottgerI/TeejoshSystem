using System.Windows;

namespace TeejoshInventario.WPF.Adapters.Inbound.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowSuccess(string message)
        {
            MessageBox.Show(
                message,
                "Exito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
