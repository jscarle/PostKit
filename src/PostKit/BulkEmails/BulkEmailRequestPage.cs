using JetBrains.Annotations;

namespace PostKit.BulkEmails;

/// <summary>Represents a page of bulk email requests.</summary>
public sealed record BulkEmailRequestPage
{
    internal BulkEmailRequestPage(IReadOnlyList<BulkEmailRequestSummary> requests, string? paginationKey)
    {
        Requests = requests;
        PaginationKey = paginationKey;
    }

    /// <summary>Gets the bulk requests on this page.</summary>
    public IReadOnlyList<BulkEmailRequestSummary> Requests { [UsedImplicitly] get; }

    /// <summary>Gets the opaque cursor for the next page, or null on the last page.</summary>
    public string? PaginationKey { [UsedImplicitly] get; }
}
