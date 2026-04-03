using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public class NotificationService : INotificationService
    {
        private async Task<Window> GetMainWindow()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow ?? throw new System.InvalidOperationException("No se encontró la ventana principal");
            throw new System.InvalidOperationException("No se pudo obtener la ventana principal");
        }

        public async Task ShowSuccess(string message, string title = "Éxito")
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Success);
            await box.ShowAsync();
        }

        public async Task ShowError(string message, string title = "Error")
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Error);
            await box.ShowAsync();
        }

        public async Task ShowWarning(string message, string title = "Advertencia")
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Warning);
            await box.ShowAsync();
        }

        public async Task ShowInfo(string message, string title = "Información")
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, Icon.Info);
            await box.ShowAsync();
        }
    }
}