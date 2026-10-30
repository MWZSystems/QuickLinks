namespace QuickLinks.Models.ViewModels.Admin;

public sealed record ApplicationRow(
    int Id,
    string Name,
    string? Image,
    string Link,
    string? Version,
    IReadOnlyList<string> Categories,
    DateTime LastUpdated,
    bool Available);
