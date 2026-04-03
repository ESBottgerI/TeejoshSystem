using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Views.Menu;
public partial class MenuPrincipalView : UserControl
{
    public MenuPrincipalView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}