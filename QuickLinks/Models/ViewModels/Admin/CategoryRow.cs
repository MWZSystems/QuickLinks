namespace QuickLinks.Models.ViewModels.Admin;

public sealed record CategoryRow(int Id, string Name, string Color, int ApplicationCount, DateTime LastUpdated, bool Available);
