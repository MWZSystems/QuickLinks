namespace QuickLinks.Services;

/// <summary>Límites de las imágenes que se suben desde la administración.</summary>
public sealed record MediaSettings(int MaxFileSizeKb, int CarouselMaxImages);
