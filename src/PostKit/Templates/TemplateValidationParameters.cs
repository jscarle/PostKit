using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents parameters for validating Postmark template content.</summary>
public sealed record TemplateValidationParameters
{
    /// <summary>Gets the subject content to validate.</summary>
    public string? Subject { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the HTML body content to validate.</summary>
    public string? HtmlBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the text body content to validate.</summary>
    public string? TextBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional model to merge when rendering test content.</summary>
    public object? TestRenderModel { [UsedImplicitly] get; init; }

    /// <summary>Gets whether CSS should be inlined for HTML test rendering.</summary>
    public bool? InlineCssForHtmlTestRender { [UsedImplicitly] get; [UsedImplicitly] init; }

    /// <summary>Gets the template type to validate. When omitted, Postmark defaults to a standard template.</summary>
    public TemplateType? TemplateType { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional layout template alias to validate with a standard template.</summary>
    public string? LayoutTemplate { [UsedImplicitly] get; init; }
}
