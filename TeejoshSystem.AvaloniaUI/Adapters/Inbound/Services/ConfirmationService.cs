using System.Windows;

namespace TeejoshInventario.WPF.Adapters.Inbound.Services
{
    public class ConfirmationService : IConfirmationService
    {
        public bool Confirm(string message, string title = "Confirmacion")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }
    }
}
