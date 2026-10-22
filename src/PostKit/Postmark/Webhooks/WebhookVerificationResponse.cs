using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LightResults;
using PostKit.Webhooks;

namespace PostKit.Postmark.Webhooks;

internal sealed class WebhookVerificationResponse
{
    [JsonPropertyName("Id")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Url")]
    public string? Url { get; [UsedImplicitly] init; }

    [JsonPropertyName("Success")]
    public bool? Success { get; [UsedImplicitly] init; }

    [JsonPropertyName("Results")]
    public IReadOnlyList<WebhookVerificationTriggerResponse?>? Results { get; [UsedImplicitly] init; }

    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }

    [JsonPropertyName("ErrorCode")]
    public int? ErrorCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Errors")]
    public System.Text.Json.Nodes.JsonNode? Errors { get; [UsedImplicitly] init; }

    internal Result<WebhookVerification> ToVerification(bool requireId)
    {
        if (requireId && Id is null or <= 0)
            return Result.Failure<WebhookVerification>("Id must be a positive identifier in the webhook verification response.");
        if (string.IsNullOrWhiteSpace(Url))
            return Result.Failure<WebhookVerification>("Url must be present in the webhook verification response.");
        if (Success is null || Results is null || Message is null)
            return Result.Failure<WebhookVerification>("Success, Results, and Message must be present in the webhook verification response.");

        var results = new WebhookVerificationTriggerResult[Results.Count];
        for (var index = 0; index < Results.Count; index++)
        {
            var result = Results[index];
            if (result is null || string.IsNullOrWhiteSpace(result.TriggerType) || result.Success is null || result.StatusCode is null or < 0 || result.Message is null)
                return Result.Failure<WebhookVerification>($"Webhook verification Results[{index}] must contain TriggerType, Success, a nonnegative StatusCode, and Message.");

            results[index] = new WebhookVerificationTriggerResult(result.TriggerType, result.Success.Value, result.StatusCode.Value, result.Message);
        }

        var verification = new WebhookVerification(Id is > 0 ? Id : null, Url, Success.Value, Array.AsReadOnly(results), Message);
        return Result.Success(verification);
    }
}
