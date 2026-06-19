namespace PostKit.Templates;

/// <summary>Identifies the kind of Postmark template.</summary>
public enum TemplateType
{
    /// <summary>A normal sendable template.</summary>
    Standard,

    /// <summary>A layout template used by standard templates.</summary>
    Layout
}