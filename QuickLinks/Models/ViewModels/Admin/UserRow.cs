namespace QuickLinks.Models.ViewModels.Admin;

public sealed record UserRow(int Id, string Name, string UserName, string Role, DateTime? LastLogin, bool IsLocked, bool Available);
