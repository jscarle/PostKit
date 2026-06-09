namespace PostKit.Suppressions;

/// <summary>Represents the per-address status returned after creating or deleting suppressions.</summary>
public enum SuppressionStatus
{
    /// <summary>The requested suppression change failed.</summary>
    Failed = 0,

    /// <summary>The address was suppressed.</summary>
    Suppressed = 1,

    /// <summary>The suppression was deleted.</summary>
    Deleted = 2,
}
