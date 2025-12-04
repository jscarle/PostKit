using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using global::PostKit;

namespace PostKit.Responses;

/// <summary>Represents the response returned after Postmark processes a batch of emails.</summary>
public sealed record SendEmailBatchResponse
{
    /// <summary>Gets the individual results for each email in the batch.</summary>
    public IReadOnlyList<SendEmailBatchResult> Results { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether every email in the batch was accepted for delivery.</summary>
    public bool IsSuccessful { [UsedImplicitly] get; }

    internal SendEmailBatchResponse(IReadOnlyList<SendEmailBatchResult> results)
    {
        Results = results switch
        {
            ReadOnlyCollection<SendEmailBatchResult> collection => collection,
            _ => new ReadOnlyCollection<SendEmailBatchResult>(results.ToList()),
        };

        IsSuccessful = Results.All(static result => result.IsSuccessful);
    }
}

/// <summary>Represents the result of attempting to send a single email within a batch.</summary>
public sealed record SendEmailBatchResult
{
    /// <summary>Gets the email that was sent.</summary>
    public Email Email { [UsedImplicitly] get; }

    /// <summary>Gets the Postmark message returned for the email.</summary>
    public string Message { [UsedImplicitly] get; }

    /// <summary>Gets the Postmark error code returned for the email.</summary>
    public int ErrorCode { [UsedImplicitly] get; }

    /// <summary>Gets the recipient string returned by Postmark.</summary>
    public string? Recipient { [UsedImplicitly] get; }

    /// <summary>Gets the time Postmark accepted the email.</summary>
    public DateTimeOffset? SubmittedAt { [UsedImplicitly] get; }

    /// <summary>Gets the successful send response, when available.</summary>
    public SendEmailResponse? Response { [UsedImplicitly] get; }

    /// <summary>Gets a value indicating whether the email was accepted by Postmark.</summary>
    public bool IsSuccessful => ErrorCode == 0 && Response is not null;

    internal SendEmailBatchResult(
        Email email,
        string message,
        int errorCode,
        string? recipient,
        DateTimeOffset? submittedAt,
        SendEmailResponse? response)
    {
        Email = email;
        Message = message;
        ErrorCode = errorCode;
        Recipient = recipient;
        SubmittedAt = submittedAt;
        Response = response;
    }
}
