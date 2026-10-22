using System.Text.Json;
using JetBrains.Annotations;

namespace PostKit.BulkEmails;

/// <summary>Represents a bulk email request returned by the list endpoint.</summary>
public sealed record BulkEmailRequestSummary
{
    internal BulkEmailRequestSummary(Guid id, BulkEmailStatus status, DateTimeOffset submittedAt, int totalMessages, double percentageCompleted, string? subject, int? releasedCount, int? failedCount, IReadOnlyDictionary<string, JsonElement>? additionalProperties)
    {
        Id = id;
        Status = status;
        SubmittedAt = submittedAt;
        TotalMessages = totalMessages;
        PercentageCompleted = percentageCompleted;
        Subject = subject;
        ReleasedCount = releasedCount;
        FailedCount = failedCount;
        AdditionalProperties = additionalProperties;
    }

    /// <summary>Gets the bulk request identifier.</summary>
    public Guid Id { [UsedImplicitly] get; }

    /// <summary>Gets the reported processing state.</summary>
    public BulkEmailStatus Status { [UsedImplicitly] get; }

    /// <summary>Gets the submission time.</summary>
    public DateTimeOffset SubmittedAt { [UsedImplicitly] get; }

    /// <summary>Gets the total number of messages.</summary>
    public int TotalMessages { [UsedImplicitly] get; }

    /// <summary>Gets the percentage processed.</summary>
    public double PercentageCompleted { [UsedImplicitly] get; }

    /// <summary>Gets the subject, when supplied.</summary>
    public string? Subject { [UsedImplicitly] get; }

    /// <summary>Gets the released count, when available as a nonnegative integer.</summary>
    public int? ReleasedCount { [UsedImplicitly] get; }

    /// <summary>Gets the failed count, when available as a nonnegative integer.</summary>
    public int? FailedCount { [UsedImplicitly] get; }

    /// <summary>Gets the additional provider fields, including the original counters.</summary>
    public IReadOnlyDictionary<string, JsonElement>? AdditionalProperties { [UsedImplicitly] get; }
}
