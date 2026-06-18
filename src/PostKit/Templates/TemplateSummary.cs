using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents a template returned from Postmark without full body content.</summary>
public record TemplateSummary
{
    internal TemplateSummary(long templateId, string name, bool active, string? alias, TemplateType templateType, string? layoutTemplate)
    {
        TemplateId = templateId;
        Name = name;
        Active = active;
        Alias = alias;
        TemplateType = templateType;
        LayoutTemplate = layoutTemplate;
    }

    /// <summary>Gets the Postmark template ID.</summary>
    public long TemplateId { [UsedImplicitly] get; }

    /// <summary>Gets the template name.</summary>
    public string Name { [UsedImplicitly] get; }

    /// <summary>Gets whether the template is active.</summary>
    public bool Active { [UsedImplicitly] get; }

    /// <summary>Gets the template alias, when one is set.</summary>
    public string? Alias { [UsedImplicitly] get; }

    /// <summary>Gets the template type.</summary>
    public TemplateType TemplateType { [UsedImplicitly] get; }

    /// <summary>Gets the associated layout template alias, when one is set.</summary>
    public string? LayoutTemplate { [UsedImplicitly] get; }
}
