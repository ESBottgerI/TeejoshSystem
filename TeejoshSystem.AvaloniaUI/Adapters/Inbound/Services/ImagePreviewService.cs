using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services;

public sealed class ImagePreviewService : IImagePreviewService
{
    public async Task ShowAsync(byte[]? image, string productName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (global::Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is not { } owner)
            return;

        var close = new Button
        {
            Content = "Cerrar",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12)
        };
        var dialog = new Window
        {
            Title = $"Imagen de {productName}",
            Width = System.Math.Max(420, owner.Bounds.Width * 0.70),
            Height = System.Math.Max(320, owner.Bounds.Height * 0.70),
            MinWidth = 420,
            MinHeight = 320,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true
        };

        Control content;
        if (image is { Length: > 0 })
        {
            using var stream = new MemoryStream(image, writable: false);
            content = new Image
            {
                Source = new Bitmap(stream),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(16)
            };
        }
        else
        {
            content = new TextBlock
            {
                Text = "Producto sin imagen",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        close.Click += (_, _) => dialog.Close();
        dialog.Opened += (_, _) => Dispatcher.UIThread.Post(() => close.Focus());
        dialog.Content = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children =
            {
                content,
                new Border { [Grid.RowProperty] = 1, Child = close }
            }
        };

        await dialog.ShowDialog(owner);
    }
}