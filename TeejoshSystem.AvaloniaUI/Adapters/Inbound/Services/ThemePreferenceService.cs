using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Styling;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public sealed class ThemePreferenceService : IThemePreferenceService
{
    private readonly string _folderPath;
    private readonly string _filePath;

    public ThemePreferenceService()
    {
        _folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TeejoshSystem");
        _filePath = Path.Combine(_folderPath, "theme-preference.txt");
    }

    public async Task<ThemeVariant> GetThemeAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return ThemeVariant.Light;

            string content = await File.ReadAllTextAsync(_filePath);
            return content.Trim() == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
        }
        catch
        {
            return ThemeVariant.Light;
        }
    }

    public async Task SaveThemeAsync(ThemeVariant theme)
    {
        try
        {
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            string content = theme == ThemeVariant.Dark ? "Dark" : "Light";
            await File.WriteAllTextAsync(_filePath, content);
        }
        catch
        {
            // No interrumpir el flujo principal de la interfaz de usuario si no se puede guardar la preferencia. (O añadir log del suceso)
        }
    }
}
