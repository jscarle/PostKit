using System.Collections.ObjectModel;
using JetBrains.Annotations;
using LightResults;

namespace PostKit.Emails;

/// <summary>Represents the response returned after Postmark processes a batch of emails.</summary>
public sealed record EmailBatchSubmission
{
    internal EmailBatchSubmission(IReadOnlyList<Result<EmailSubmission>> results)
    {
        Results = results switch
        {
            ReadOnlyCollection<Result<EmailSubmission>> collection => collection,
            _ => new ReadOnlyCollection<Result<EmailSubmission>>(results.ToList())
        };

        IsSuccessful = Results.All(static result => result.IsSuccess());
    }

    /// <summary>Gets a value indicating whether every email in the batch was accepted for delivery.</summary>
    public bool IsSuccessful { [UsedImplicitly] get; }

    /// <summary>Gets the individual results for each email in the batch.</summary>
    public IReadOnlyList<Result<EmailSubmission>> Results { [UsedImplicitly] get; }
}