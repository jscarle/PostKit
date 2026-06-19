namespace PostKit.Templates;

/// <summary>Identifies the change Postmark would make or made during a template push.</summary>
public enum TemplatePushAction
{
    /// <summary>The destination template would be created.</summary>
    Create,

    /// <summary>The destination template would be edited.</summary>
    Edit
}