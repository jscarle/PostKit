namespace PostKit.Configuration;

/// <summary>
/// Represents configuration settings used to access the PostKit/Postmark API.
/// </summary>
public sealed class PostKitOptions
{
    /// <summary>
    /// Gets the API token used for account level operations.
    /// </summary>
    public string? AccountApiToken { get; init; }

    /// <summary>
    /// Gets the API token used for server level operations.
    /// </summary>
    public string? ServerApiToken { get; init; }
}
