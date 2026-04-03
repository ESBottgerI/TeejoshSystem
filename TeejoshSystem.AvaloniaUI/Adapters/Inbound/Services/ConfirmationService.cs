using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public class ConfirmationService : IConfirmationService
    {
        private Window? GetMainWindow()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow;
            return null;
        }

        public async Task<bool> Confirm(string message, string title = "Confirmar")
        {
            var window = GetMainWindow();
            if (window == null)
                return false;

            var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo, Icon.Question);
            var result = await box.ShowAsync();
            return result == ButtonResult.Yes;
        }
    }
}