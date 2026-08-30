using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents parameters for editing a Postmark template.</summary>
public sealed record TemplateEditParameters
{
    /// <summary>Gets the template name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional template alias.</summary>
    public string? Alias { [UsedImplicitly] get; init; }

    /// <summary>Gets the subject content.</summary>
    public string? Subject { [UsedImplicitly] get; init; }

    /// <summary>Gets the HTML body content.</summary>
    public string? HtmlBody { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the text body content.</summary>
    public string? TextBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional layout template alias to apply to a standard template.</summary>
    public string? LayoutTemplate { [UsedImplicitly] get; [UsedImplicitly] init; }
}
