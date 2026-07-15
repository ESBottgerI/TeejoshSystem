using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Svg.Skia;
using SkiaSharp;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Storage
{
    public class LocalImageStorageService : IImageStorageService
    {
        private readonly string _imagesFolder;
        private static readonly HttpClient _http = new();

        public LocalImageStorageService(string? imagesFolder = null)
        {
            _imagesFolder = imagesFolder ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeejoshSystem",
                "images");

            Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<string?> SaveImageAsync(string? rutaOrigen)
        {
            if (string.IsNullOrWhiteSpace(rutaOrigen))
                return null;

            if (!File.Exists(rutaOrigen))
                return null;

            var extension = Path.GetExtension(rutaOrigen).ToLowerInvariant();
            if (extension is not (".png" or ".jpg" or ".jpeg" or ".gif" or ".webp"))
                return null;
            var bytes = await File.ReadAllBytesAsync(rutaOrigen);
            if (bytes.Length == 0 || bytes.Length > 10 * 1024 * 1024)
                return null;
            try
            {
                try
                {
                    using var decoded = SKBitmap.Decode(bytes);
                    if (decoded is null) return null;
                }
                catch
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);
            await File.WriteAllBytesAsync(rutaDestino, bytes);
            return nombreArchivo;
        }

        public string? GetFullPath(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName) || Path.GetFileName(imageName) != imageName)
                return null;

            var fullPath = Path.GetFullPath(Path.Combine(_imagesFolder, imageName));
            var root = Path.GetFullPath(_imagesFolder) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath)
                ? fullPath
                : null;
        }

        public async Task<StoredImageContent?> ReadImageAsync(string? imageName, bool thumbnail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imageName) || Path.GetFileName(imageName) != imageName)
                return null;
            var fullPath = Path.GetFullPath(Path.Combine(_imagesFolder, imageName));
            var root = Path.GetFullPath(_imagesFolder) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
                return null;
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".gif" => "image/gif", ".webp" => "image/webp", _ => null };
            if (contentType is null) return null;
            var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            using var source = SKBitmap.Decode(bytes);
            if (source is null) return null;
            if (!thumbnail) return new StoredImageContent(bytes, contentType);
            const int side = 48;
            var scale = Math.Min((float)side / source.Width, (float)side / source.Height);
            var width = Math.Max(1, (int)(source.Width * scale));
            var height = Math.Max(1, (int)(source.Height * scale));
            using var resized = source.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            if (resized is null) return null;
            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 90);
            return new StoredImageContent(encoded.ToArray(), "image/png");
        }

        public async Task<string?> SaveImageFromUrlAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || uri.IsLoopback)
                    return null;
                using var response = await _http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength is > 10485760)
                    return null;
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (bytes.Length == 0 || bytes.Length > 10 * 1024 * 1024)
                    return null;
                var extension = Path.GetExtension(
                    uri.AbsolutePath).ToLowerInvariant();

                // Si es SVG, convertir a PNG antes de guardar
                if (extension == ".svg")
                    return await ConvertirSvgAPngAsync(bytes);

                if (extension is not (".png" or ".jpg" or ".jpeg" or ".gif" or ".webp"))
                    return null;
                try
                {
                    using var decoded = SKBitmap.Decode(bytes);
                    if (decoded is null) return null;
                }
                catch
                {
                    return null;
                }

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);

                await File.WriteAllBytesAsync(rutaDestino, bytes);
                return nombreArchivo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"SaveImageFromUrlAsync error: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> ConvertirSvgAPngAsync(byte[] svgBytes)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var svg = new SKSvg();
                    using var stream = new MemoryStream(svgBytes);
                    svg.Load(stream);

                    if (svg.Picture is null)
                        return null;

                    var width = 256;
                    var height = 256;

                    var imageInfo = new SKImageInfo(width, height,
                        SKColorType.Rgba8888, SKAlphaType.Premul);

                    using var surface = SKSurface.Create(imageInfo);
                    var canvas = surface.Canvas;

                    canvas.Clear(SKColors.Transparent);

                    var bounds = svg.Picture.CullRect;
                    var scaleX = width / bounds.Width;
                    var scaleY = height / bounds.Height;
                    var scale = Math.Min(scaleX, scaleY);

                    canvas.Scale(scale);
                    canvas.DrawPicture(svg.Picture);

                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                    var nombreArchivo = $"{Guid.NewGuid()}.png";
                    var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);

                    File.WriteAllBytes(rutaDestino, data.ToArray());
                    return nombreArchivo;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ConvertirSvgAPngAsync error: {ex.Message}");
                return null;
            }
        }
    }
}
