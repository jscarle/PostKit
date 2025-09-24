using JetBrains.Annotations;

namespace PostKit.Responses;

public sealed record SendEmailResponse
{
    public string MessageId { [UsedImplicitly] get; }

    internal SendEmailResponse(string messageId)
    {
        MessageId = $"<{messageId}@mtasv.net>";
    }
}
