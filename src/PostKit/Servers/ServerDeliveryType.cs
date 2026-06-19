namespace PostKit.Servers;

/// <summary>Identifies whether a Postmark server sends live email or sandbox test email.</summary>
public enum ServerDeliveryType
{
    /// <summary>The server sends live email.</summary>
    Live,

    /// <summary>The server is sandboxed.</summary>
    Sandbox
}