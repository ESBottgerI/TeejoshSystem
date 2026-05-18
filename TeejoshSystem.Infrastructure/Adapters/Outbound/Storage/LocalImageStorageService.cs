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

        public LocalImageStorageService()
        {
            _imagesFolder = Path.Combine(
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
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaDestino = Path.Combine(_imagesFolder, nombreArchivo);

            await Task.Run(() => File.Copy(rutaOrigen, rutaDestino, overwrite: true));

            return nombreArchivo;
        }

        public string? GetFullPath(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return null;

            var fullPath = Path.Combine(_imagesFolder, imageName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        public async Task<string?> SaveImageFromUrlAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var bytes = await _http.GetByteArrayAsync(url);
                var extension = Path.GetExtension(
                    new Uri(url).AbsolutePath).ToLowerInvariant();

                // Si es SVG, convertir a PNG antes de guardar
                if (extension == ".svg")
                    return await ConvertirSvgAPngAsync(bytes);

                if (string.IsNullOrWhiteSpace(extension))
                    extension = ".png";

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