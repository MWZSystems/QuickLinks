using Microsoft.Extensions.Options;

namespace QuickLinks.Services;

/// <summary>
/// Guarda logos e imágenes fuera de wwwroot con nombres generados (GUID) y validación de tipo y tamaño.
/// </summary>
public sealed class MediaStorage : IMediaStorage
{
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = [[0x89, 0x50, 0x4E, 0x47]],
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".gif"] = [[0x47, 0x49, 0x46, 0x38]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]],
        [".svg"] = []
    };

    private readonly MediaOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MediaStorage(IOptions<MediaOptions> options, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
        PhysicalRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.RootPath));
        foreach (var folder in Enum.GetValues<MediaFolder>())
        {
            Directory.CreateDirectory(Path.Combine(PhysicalRoot, folder.ToString()));
        }
    }

    public string PhysicalRoot { get; }

    public async Task<(string? FileName, string? Error)> SaveImageAsync(IFormFile file, MediaFolder folder, int maxSizeKb, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!Signatures.TryGetValue(extension, out var signatures))
        {
            return (null, "Formato no permitido. Usa PNG, JPG, GIF, WEBP o SVG.");
        }

        if (file.Length == 0 || file.Length > maxSizeKb * 1024L)
        {
            return (null, $"La imagen debe pesar menos de {maxSizeKb} KB.");
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[512];
        var read = await stream.ReadAsync(header, ct);

        if (!HasValidContent(extension, signatures, header.AsSpan(0, read)))
        {
            return (null, "El contenido del archivo no corresponde a una imagen válida.");
        }

        stream.Position = 0;
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(PhysicalRoot, folder.ToString(), fileName);
        await using var output = File.Create(path);
        await stream.CopyToAsync(output, ct);

        return (fileName, null);
    }

    public void Delete(MediaFolder folder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        // Path.GetFileName evita que un nombre manipulado salga de la carpeta de medios.
        var path = Path.Combine(PhysicalRoot, folder.ToString(), Path.GetFileName(fileName));
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public string GetUrl(MediaFolder folder, string fileName)
    {
        var pathBase = _httpContextAccessor.HttpContext?.Request.PathBase.Value ?? string.Empty;
        return $"{pathBase}{_options.RequestPath}/{folder}/{Uri.EscapeDataString(fileName)}";
    }

    private static bool HasValidContent(string extension, byte[][] signatures, ReadOnlySpan<byte> header)
    {
        if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            var text = System.Text.Encoding.UTF8.GetString(header).TrimStart('﻿', ' ', '\r', '\n', '\t');
            return text.StartsWith('<') && text.Contains("<svg", StringComparison.OrdinalIgnoreCase);
        }

        foreach (var signature in signatures)
        {
            if (header.StartsWith(signature))
            {
                return true;
            }
        }

        return false;
    }
}
