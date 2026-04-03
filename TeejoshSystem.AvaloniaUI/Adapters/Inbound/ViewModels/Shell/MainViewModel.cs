using CommunityToolkit.Mvvm.ComponentModel;
using static TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Common.ValidatableViewModel;

namespace TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Shell
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? _currentView;
        partial void OnCurrentViewChanged(object? value)
        {
            if (value is ILoadable loadable)
                loadable.OnLoaded();
        }
    }
}
