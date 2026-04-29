namespace PostKit.BulkEmails;

/// <summary>Represents the processing state of a Postmark bulk email request.</summary>
public enum BulkEmailStatus
{
    /// <summary>The bulk email request has been accepted.</summary>
    Accepted,

    /// <summary>The bulk email request is still being processed.</summary>
    Processing,

    /// <summary>The bulk email request has completed processing.</summary>
    Completed,

    /// <summary>The bulk email request failed during processing.</summary>
    Failed,
}
