namespace QuickLinks.Services;

public interface IMediaStorage
{
    string PhysicalRoot { get; }

    /// <summary>Valida y guarda una imagen; devuelve el nombre de archivo generado o el error de validación.</summary>
    Task<(string? FileName, string? Error)> SaveImageAsync(IFormFile file, MediaFolder folder, CancellationToken ct = default);

    void Delete(MediaFolder folder, string? fileName);

    string GetUrl(MediaFolder folder, string fileName);
}
