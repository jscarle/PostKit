using JetBrains.Annotations;

namespace PostKit.BulkEmails;

/// <summary>Represents the status returned for a Postmark bulk email request.</summary>
public sealed record BulkEmailJob
{
    /// <summary>Gets the identifier assigned to the bulk email request.</summary>
    public Guid Id { [UsedImplicitly] get; }

    /// <summary>Gets the processing state reported by Postmark.</summary>
    public BulkEmailStatus Status { [UsedImplicitly] get; }

    /// <summary>Gets the time Postmark accepted the bulk email request.</summary>
    public DateTimeOffset SubmittedAt { [UsedImplicitly] get; }

    /// <summary>Gets the total number of messages in the bulk request.</summary>
    public int TotalMessages { [UsedImplicitly] get; }

    /// <summary>Gets the percentage of messages processed so far.</summary>
    public double PercentageCompleted { [UsedImplicitly] get; }

    /// <summary>Gets the subject associated with the bulk request.</summary>
    public string Subject { [UsedImplicitly] get; }

    internal BulkEmailJob(Guid id, BulkEmailStatus status, DateTimeOffset submittedAt, int totalMessages, double percentageCompleted, string subject)
    {
        Id = id;
        Status = status;
        SubmittedAt = submittedAt;
        TotalMessages = totalMessages;
        PercentageCompleted = percentageCompleted;
        Subject = subject;
    }
}
