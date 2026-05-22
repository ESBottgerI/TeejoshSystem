using System.Threading.Tasks;
using Avalonia.Styling;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public interface IThemePreferenceService
{
    Task<ThemeVariant> GetThemeAsync();
    Task SaveThemeAsync(ThemeVariant theme);
}
