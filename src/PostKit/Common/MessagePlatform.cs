using System.Text.Json.Serialization;
using PostKit.Postmark.Common;

namespace PostKit.Common;

/// <summary>Identifies the platform reported for a tracked message event.</summary>
[JsonConverter(typeof(MessagePlatformJsonConverter))]
public enum MessagePlatform
{
    /// <summary>The WebMail value reported by Postmark.</summary>
    WebMail = 0,

    /// <summary>The Desktop value reported by Postmark.</summary>
    Desktop = 1,

    /// <summary>The Mobile value reported by Postmark.</summary>
    Mobile = 2,

    /// <summary>The Unknown value reported by Postmark.</summary>
    Unknown = 3
}
