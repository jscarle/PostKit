using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents the raw dump returned for a bounce.</summary>
public sealed record BounceDump
{
    internal BounceDump(string body)
    {
        Body = body;
    }

    /// <summary>Gets the raw dump body. Postmark returns an empty string when no dump is available.</summary>
    public string Body { [UsedImplicitly] get; }
}