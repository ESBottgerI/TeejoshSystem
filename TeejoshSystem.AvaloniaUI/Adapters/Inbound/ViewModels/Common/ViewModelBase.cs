using CommunityToolkit.Mvvm.ComponentModel;

namespace TeejoshInventario.WPF.Adapters.Inbound.ViewModels.Common
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;
    }
}
