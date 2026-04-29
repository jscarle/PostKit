using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents the response returned after reactivating a bounce.</summary>
public sealed record BounceActivation
{
    /// <summary>Gets the status message returned by Postmark.</summary>
    public string Message { [UsedImplicitly] get; }

    /// <summary>Gets the bounce returned by Postmark for the activation request.</summary>
    public Bounce Bounce { [UsedImplicitly] get; }

    internal BounceActivation(string message, Bounce bounce)
    {
        Message = message;
        Bounce = bounce;
    }
}
