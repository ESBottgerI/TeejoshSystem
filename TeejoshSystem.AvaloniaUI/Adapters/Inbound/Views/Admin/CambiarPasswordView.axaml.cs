using Avalonia.Controls;
using Avalonia.Interactivity;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Views.Admin
{
    public partial class CambiarPasswordView : UserControl
    {
        public CambiarPasswordView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (DataContext is CambiarPasswordViewModel vm)
            {
                PasswordActualBox.TextChanged += (_, _) =>
                    vm.ActualizarPasswordActual(PasswordActualBox.Text ?? string.Empty);

                PasswordNuevoBox.TextChanged += (_, _) =>
                    vm.ActualizarPasswordNuevo(PasswordNuevoBox.Text ?? string.Empty);

                ConfirmarPasswordBox.TextChanged += (_, _) =>
                    vm.ActualizarConfirmarPassword(ConfirmarPasswordBox.Text ?? string.Empty);
            }
        }
    }
}