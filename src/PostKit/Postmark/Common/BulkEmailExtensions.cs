using System.Diagnostics;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Postmark.Bulk;
using PostKit.Postmark.Email;

namespace PostKit.Postmark.Common;

internal static class BulkEmailExtensions
{
    public static BulkEmailRequest ToBulkEmailRequest(this BulkEmail bulkEmail)
    {
        if (bulkEmail.From is null)
            throw new UnreachableException($"{nameof(bulkEmail.From)} is unexpectedly null.");

        var replyTo = bulkEmail.ReplyTo is not null ? string.Join(",", bulkEmail.ReplyTo.Select(static x => x.ToString(true))) : null;
        IReadOnlyList<EmailRequestAttachment>? attachments = bulkEmail.Attachments?.Select(static attachment => new EmailRequestAttachment
            {
                Name = attachment.Name, ContentType = attachment.ContentType, Content = attachment.Content, ContentId = attachment.ContentId
            })
            .ToList();
        IReadOnlyList<EmailRequestHeader>? headers = bulkEmail.Headers?.Select(static x => new EmailRequestHeader { Name = x.Key, Value = x.Value })
            .ToList();
        IReadOnlyDictionary<string, string>? metadata = bulkEmail.Metadata?.ToDictionary();

        var trackLinks = bulkEmail.LinkTracking is not null
            ? bulkEmail.LinkTracking switch
            {
                LinkTracking.None => "None",
                LinkTracking.HtmlAndText => "HtmlAndText",
                LinkTracking.HtmlOnly => "HtmlOnly",
                LinkTracking.TextOnly => "TextOnly",
                _ => throw new UnreachableException($"Enum value of '{nameof(LinkTracking)}.{bulkEmail.LinkTracking}' has not been handled.")
            }
            : null;

        return new BulkEmailRequest
        {
            From = bulkEmail.From.ToString(true),
            ReplyTo = replyTo,
            Subject = bulkEmail.Subject,
            HtmlBody = bulkEmail.HtmlBody,
            TextBody = bulkEmail.TextBody,
            TemplateId = bulkEmail.TemplateId,
            TemplateAlias = bulkEmail.TemplateAlias,
            InlineCss = bulkEmail.InlineCss,
            Tag = bulkEmail.Tag,
            Metadata = metadata,
            MessageStream = bulkEmail.MessageStream,
            TrackOpens = bulkEmail.OpenTracking,
            TrackLinks = trackLinks,
            Attachments = attachments,
            Headers = headers,
            Messages = bulkEmail.Messages.Select(ToBulkEmailMessageRequest)
                .ToList()
        };
    }

    private static BulkEmailMessageRequest ToBulkEmailMessageRequest(BulkEmailMessage message)
    {
        var to = message.To is not null ? string.Join(",", message.To.Select(static x => x.ToString(true))) : null;
        var cc = message.Cc is not null ? string.Join(",", message.Cc.Select(static x => x.ToString(true))) : null;
        var bcc = message.Bcc is not null ? string.Join(",", message.Bcc.Select(static x => x.ToString(true))) : null;
        IReadOnlyList<EmailRequestHeader>? headers = message.Headers?.Select(static x => new EmailRequestHeader { Name = x.Key, Value = x.Value })
            .ToList();
        IReadOnlyDictionary<string, string>? metadata = message.Metadata?.ToDictionary();

        return new BulkEmailMessageRequest
        {
            To = to,
            Cc = cc,
            Bcc = bcc,
            TemplateModel = message.TemplateModelNode,
            Metadata = metadata,
            Headers = headers
        };
    }
}