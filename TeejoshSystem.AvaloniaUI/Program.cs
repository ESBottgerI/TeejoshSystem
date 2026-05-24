using Avalonia;
using Serilog;
using System;

namespace TeejoshSystem.AvaloniaUI;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Bootstrap logger mínimo para capturar errores ANTES de que Avalonia arranque.
        // Se reemplaza por la config completa de appsettings.json dentro de App.axaml.cs.
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación terminó inesperadamente.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
