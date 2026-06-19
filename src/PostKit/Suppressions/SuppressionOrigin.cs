namespace PostKit.Suppressions;

/// <summary>Represents the source that added an address to a Postmark suppression list.</summary>
public enum SuppressionOrigin
{
    /// <summary>The recipient caused the suppression.</summary>
    Recipient = 0,

    /// <summary>The customer caused the suppression.</summary>
    Customer = 1,

    /// <summary>A Postmark administrator caused the suppression.</summary>
    Admin = 2
}