using System.Text.Json;
using LightResults;
using PostKit.BulkEmails;

namespace PostKit.Errors;

/// <summary>Represents a bulk email submission that was accepted by Postmark but included unprocessable messages.</summary>
public sealed class BulkEmailValidationError : Error
{
    /// <summary>Gets the accepted bulk job returned by Postmark.</summary>
    public BulkEmailJob AcceptedJob { get; }

    /// <summary>Gets the server-provided details for the unprocessable messages.</summary>
    public JsonElement UnprocessableContent { get; }

    internal BulkEmailValidationError(BulkEmailJob acceptedJob, JsonElement unprocessableContent)
        : base("The Postmark Bulk API accepted the bulk email request but reported unprocessable messages.")
    {
        AcceptedJob = acceptedJob;
        UnprocessableContent = unprocessableContent.Clone();
    }
}
