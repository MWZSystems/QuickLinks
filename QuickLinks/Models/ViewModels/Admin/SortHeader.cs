namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Encabezado de columna ordenable en un listado.</summary>
public sealed record SortHeader(ListQuery Query, string Column, string Label)
{
    public bool IsActive => string.Equals(Query.Sort, Column, StringComparison.OrdinalIgnoreCase);

    public bool NextDesc => IsActive && !Query.Desc;
}
