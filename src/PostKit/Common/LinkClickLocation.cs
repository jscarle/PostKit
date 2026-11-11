using System.Text.Json.Serialization;
using PostKit.Postmark.Common;

namespace PostKit.Common;

/// <summary>Identifies the message body containing a clicked link.</summary>
[JsonConverter(typeof(LinkClickLocationJsonConverter))]
public enum LinkClickLocation
{
    /// <summary>The Html value reported by Postmark.</summary>
    Html = 0,

    /// <summary>The Text value reported by Postmark.</summary>
    Text = 1
}
