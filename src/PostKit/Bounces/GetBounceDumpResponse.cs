using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents the raw dump returned for a bounce.</summary>
public sealed record GetBounceDumpResponse
{
    /// <summary>Gets the raw dump body. Postmark returns an empty string when no dump is available.</summary>
    public string Body { [UsedImplicitly] get; }

    internal GetBounceDumpResponse(string body)
    {
        Body = body;
    }
}
