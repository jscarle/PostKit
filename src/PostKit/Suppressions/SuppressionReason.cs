namespace PostKit.Suppressions;

/// <summary>Represents the reason an address is suppressed in a Postmark message stream.</summary>
public enum SuppressionReason
{
    /// <summary>The recipient was suppressed because of a hard bounce.</summary>
    HardBounce = 0,

    /// <summary>The recipient was suppressed because they reported a message as spam.</summary>
    SpamComplaint = 1,

    /// <summary>The recipient was manually suppressed by the recipient, customer, administrator, or API.</summary>
    ManualSuppression = 2,
}
