using System.Collections.ObjectModel;
using LightResults;
using JetBrains.Annotations;

namespace PostKit.Responses;

/// <summary>Represents the response returned after Postmark processes a batch of emails.</summary>
public sealed record SendEmailBatchResponse
{
    /// <summary>Gets a value indicating whether every email in the batch was accepted for delivery.</summary>
    public bool IsSuccessful { [UsedImplicitly] get; }

    /// <summary>Gets the individual results for each email in the batch.</summary>
    public IReadOnlyList<Result<SendEmailResponse>> Results { [UsedImplicitly] get; }

    internal SendEmailBatchResponse(IReadOnlyList<Result<SendEmailResponse>> results)
    {
        Results = results switch
        {
            ReadOnlyCollection<Result<SendEmailResponse>> collection => collection,
            _ => new ReadOnlyCollection<Result<SendEmailResponse>>(results.ToList()),
        };

        IsSuccessful = Results.All(static result => result.IsSuccess());
    }
}
