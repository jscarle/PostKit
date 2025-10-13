namespace PostKit;

/// <summary>Represents the available link tracking modes supported by Postmark.</summary>
public enum LinkTracking
{
    /// <summary>Disables link tracking.</summary>
    None = 0,

    /// <summary>Enables link tracking for both HTML and plain text bodies.</summary>
    HtmlAndText = 1,

    /// <summary>Enables link tracking for the HTML body only.</summary>
    HtmlOnly = 2,

    /// <summary>Enables link tracking for the plain text body only.</summary>
    TextOnly = 3,
}
