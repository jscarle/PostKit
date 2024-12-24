using System.Diagnostics;
using PostKit.Postmark.Email;

namespace PostKit.Common;

internal static class EmailExtensions
{
    public static EmailRequest ToEmailRequest(this Email email)
    {
        if (email.From is null)
            throw new UnreachableException($"{nameof(email.From)} is unexpectedly null.");

        var from = email.From.ToString(true);
        var replyTo = email.ReplyTo is not null ? string.Join(",", email.ReplyTo.Select(x => x.ToString(true))) : null;
        var to = email.To is not null ? string.Join(",", email.To.Select(x => x.ToString(true))) : null;
        var cc = email.Cc is not null ? string.Join(",", email.Cc.Select(x => x.ToString(true))) : null;
        var bcc = email.Bcc is not null ? string.Join(",", email.Bcc.Select(x => x.ToString(true))) : null;
        var headers = email.Headers?.ToList();
        var metadata = email.Metadata?.ToDictionary();
        var trackLinks = email.LinkTracking is not null
            ? email.LinkTracking switch
            {
                LinkTracking.None => "None",
                LinkTracking.HtmlAndText => "HtmlAndText",
                LinkTracking.HtmlOnly => "HtmlOnly",
                LinkTracking.TextOnly => "TextOnly",
                _ => throw new UnreachableException($"Enum value of '{nameof(LinkTracking)}.{email.LinkTracking}' has not been handled."),
            }
            : null;

        var request = new EmailRequest
        {
            From = from,
            ReplyTo = replyTo,
            To = to,
            Cc = cc,
            Bcc = bcc,
            Subject = email.Subject,
            HtmlBody = email.HtmlBody,
            TextBody = email.TextBody,
            Tag = email.Tag,
            Headers = headers,
            Metadata = metadata,
            TrackOpens = email.OpenTracking,
            TrackLinks = trackLinks,
            MessageStream = email.MessageStream,
        };
        return request;
    }
}
