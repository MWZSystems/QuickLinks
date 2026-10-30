namespace QuickLinks.Services;

public sealed class MediaOptions
{
    public const string SectionName = "Media";

    /// <summary>Carpeta física donde se guardan los archivos; relativa a la raíz del contenido o absoluta.</summary>
    public string RootPath { get; set; } = "App_Data/media";

    /// <summary>Ruta pública bajo la que se sirven los archivos.</summary>
    public string RequestPath { get; set; } = "/media";

    public int MaxFileSizeKb { get; set; } = 2048;
}
