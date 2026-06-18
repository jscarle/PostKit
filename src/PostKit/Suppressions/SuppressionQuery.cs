namespace PostKit.Suppressions;

/// <summary>Represents the query parameters used to retrieve Postmark suppressions for a message stream.</summary>
public sealed record SuppressionQuery
{
    /// <summary>Gets the optional suppression reason filter.</summary>
    public SuppressionReason? Reason { get; init; }

    /// <summary>Gets the optional origin filter.</summary>
    public SuppressionOrigin? Origin { get; init; }

    /// <summary>Gets the optional upper bound for the suppression creation date.</summary>
    public DateOnly? ToDate { get; init; }

    /// <summary>Gets the optional lower bound for the suppression creation date.</summary>
    public DateOnly? FromDate { get; init; }

    /// <summary>Gets the optional email address filter.</summary>
    public string? EmailAddress { get; init; }
}
