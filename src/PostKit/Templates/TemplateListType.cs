namespace PostKit.Templates;

/// <summary>Identifies template types that can be returned by a template list query.</summary>
public enum TemplateListType
{
    /// <summary>Includes standard and layout templates.</summary>
    All,

    /// <summary>Includes only standard templates.</summary>
    Standard,

    /// <summary>Includes only layout templates.</summary>
    Layout
}
