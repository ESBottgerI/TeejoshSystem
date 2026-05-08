using Avalonia.Controls;
using Avalonia.Interactivity;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Auth;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Views.Auth
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            // Conectar el TextBox de contraseña al ViewModel.
            // Debe hacerse en OnLoaded — en el constructor, DataContext aún no está asignado.
            if (DataContext is LoginViewModel vm)
            {
                PasswordBox.TextChanged += (_, _) =>
                    vm.ActualizarPassword(PasswordBox.Text ?? string.Empty);
            }
        }
    }
}