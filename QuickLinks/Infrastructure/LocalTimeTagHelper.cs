using Microsoft.AspNetCore.Razor.TagHelpers;
using QuickLinks.Services;

namespace QuickLinks.Infrastructure;

/// <summary>
/// <c>&lt;local-time value="..." /&gt;</c>: muestra una fecha UTC de la base de datos en la zona horaria configurada.
/// </summary>
[HtmlTargetElement("local-time", TagStructure = TagStructure.WithoutEndTag)]
public sealed class LocalTimeTagHelper(ISettingsService settings) : TagHelper
{
    public DateTime? Value { get; set; }

    public string Format { get; set; } = "dd/MM/yyyy HH:mm";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "time";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (Value is not DateTime utc)
        {
            output.Content.SetContent("—");
            return;
        }

        var portal = await settings.GetAsync();
        output.Attributes.SetAttribute("datetime", DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("O"));
        output.Content.SetContent(portal.ToLocal(utc).ToString(Format));
    }
}
