using Avalonia.Controls;
using Avalonia.Interactivity;
using TeejoshSystem.AvaloniaUI.Adapters.Inbound.ViewModels.Admin;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Views.Admin
{
    public partial class GestionarUsuariosView : UserControl
    {
        public GestionarUsuariosView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            if (DataContext is GestionarUsuariosViewModel vm)
            {
                NuevaPasswordBox.TextChanged += (_, _) =>
                    vm.ActualizarNuevaPassword(NuevaPasswordBox.Text ?? string.Empty);

                ConfirmarPasswordBox.TextChanged += (_, _) =>
                    vm.ActualizarConfirmarPassword(ConfirmarPasswordBox.Text ?? string.Empty);
            }
        }
    }
}