using System.Globalization;
using System.Text;
using MimeKit;
using PostKit.BulkEmails;
using PostKit.Common;

namespace PostKit.Postmark.Common;

internal static class PostmarkSizeEstimator
{
    internal const long BodySizeLimitInBytes = 5L * 1024 * 1024;
    internal const long MessageSizeLimitInBytes = 10L * 1024 * 1024;
    internal const long BulkPayloadSizeLimitInBytes = 50L * 1024 * 1024;
    internal const long BatchPayloadSizeLimitInBytes = 50L * 1024 * 1024;

    internal static long EstimateBodySizeLowerBound(string? body)
    {
        return body is null ? 0 : Encoding.UTF8.GetByteCount(body);
    }

    internal static long EstimateBase64SizeLowerBound(string? base64Content)
    {
        if (string.IsNullOrEmpty(base64Content))
            return 0;

        return base64Content.Length;
    }

    internal static long EstimateAttachmentsSizeLowerBound(IReadOnlyCollection<Attachment>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
            return 0;

        long total = 0;
        foreach (var attachment in attachments)
        {
            total += EstimateBase64SizeLowerBound(attachment.Content);
            total += EstimateStringSizeLowerBound(attachment.Name);
            total += EstimateStringSizeLowerBound(attachment.ContentType);
            total += EstimateStringSizeLowerBound(attachment.ContentId);
        }

        return total;
    }

    internal static long EstimateMessageSizeLowerBound(Emails.Email email)
    {
        return EstimateMailboxAddressSizeLowerBound(email.From)
            + EstimateMailboxAddressesSizeLowerBound(email.ReplyTo)
            + EstimateMailboxAddressesSizeLowerBound(email.To)
            + EstimateMailboxAddressesSizeLowerBound(email.Cc)
            + EstimateMailboxAddressesSizeLowerBound(email.Bcc)
            + EstimateStringSizeLowerBound(email.Subject)
            + EstimateBodySizeLowerBound(email.TextBody)
            + EstimateBodySizeLowerBound(email.HtmlBody)
            + EstimateStringSizeLowerBound(email.Tag)
            + EstimateKeyValuePairsSizeLowerBound(email.Headers)
            + EstimateKeyValuePairsSizeLowerBound(email.Metadata)
            + EstimateBooleanSizeLowerBound(email.OpenTracking)
            + EstimateLinkTrackingSizeLowerBound(email.LinkTracking)
            + EstimateStringSizeLowerBound(email.MessageStream)
            + EstimateAttachmentsSizeLowerBound(email.Attachments)
            + EstimateIntegerSizeLowerBound(email.TemplateId)
            + EstimateStringSizeLowerBound(email.TemplateAlias)
            + email.TemplateModelSizeInBytes
            + EstimateBooleanSizeLowerBound(email.InlineCss);
    }

    internal static long EstimateBulkEmailSizeLowerBound(BulkEmail bulkEmail)
    {
        long total = EstimateMailboxAddressSizeLowerBound(bulkEmail.From)
            + EstimateMailboxAddressesSizeLowerBound(bulkEmail.ReplyTo)
            + EstimateStringSizeLowerBound(bulkEmail.Subject)
            + EstimateBodySizeLowerBound(bulkEmail.TextBody)
            + EstimateBodySizeLowerBound(bulkEmail.HtmlBody)
            + EstimateStringSizeLowerBound(bulkEmail.Tag)
            + EstimateKeyValuePairsSizeLowerBound(bulkEmail.Headers)
            + EstimateKeyValuePairsSizeLowerBound(bulkEmail.Metadata)
            + EstimateBooleanSizeLowerBound(bulkEmail.OpenTracking)
            + EstimateLinkTrackingSizeLowerBound(bulkEmail.LinkTracking)
            + EstimateStringSizeLowerBound(bulkEmail.MessageStream)
            + EstimateAttachmentsSizeLowerBound(bulkEmail.Attachments)
            + EstimateIntegerSizeLowerBound(bulkEmail.TemplateId)
            + EstimateStringSizeLowerBound(bulkEmail.TemplateAlias)
            + EstimateBooleanSizeLowerBound(bulkEmail.InlineCss);

        foreach (var message in bulkEmail.Messages)
        {
            total += EstimateMailboxAddressesSizeLowerBound(message.To);
            total += EstimateMailboxAddressesSizeLowerBound(message.Cc);
            total += EstimateMailboxAddressesSizeLowerBound(message.Bcc);
            total += message.TemplateModelSizeInBytes;
            total += EstimateKeyValuePairsSizeLowerBound(message.Metadata);
            total += EstimateKeyValuePairsSizeLowerBound(message.Headers);
        }

        return total;
    }

    internal static long EstimateStringSizeLowerBound(string? value)
    {
        return value is null ? 0 : Encoding.UTF8.GetByteCount(value);
    }

    private static long EstimateMailboxAddressSizeLowerBound(MailboxAddress? mailboxAddress)
    {
        return mailboxAddress is null ? 0 : EstimateStringSizeLowerBound(mailboxAddress.ToString(true));
    }

    private static long EstimateMailboxAddressesSizeLowerBound(IReadOnlyCollection<MailboxAddress>? mailboxAddresses)
    {
        if (mailboxAddresses is null || mailboxAddresses.Count == 0)
            return 0;

        long total = mailboxAddresses.Count - 1;
        foreach (var mailboxAddress in mailboxAddresses)
            total += EstimateMailboxAddressSizeLowerBound(mailboxAddress);

        return total;
    }

    private static long EstimateKeyValuePairsSizeLowerBound(IReadOnlyDictionary<string, string>? values)
    {
        if (values is null || values.Count == 0)
            return 0;

        long total = 0;
        foreach (var (key, value) in values)
        {
            total += EstimateStringSizeLowerBound(key);
            total += EstimateStringSizeLowerBound(value);
        }

        return total;
    }

    private static long EstimateBooleanSizeLowerBound(bool? value)
    {
        return value switch
        {
            true => 4,
            false => 5,
            null => 0,
        };
    }

    private static long EstimateIntegerSizeLowerBound(int? value)
    {
        if (!value.HasValue)
            return 0;

        return value.Value.ToString(CultureInfo.InvariantCulture).Length;
    }

    private static long EstimateLinkTrackingSizeLowerBound(LinkTracking? linkTracking)
    {
        return linkTracking switch
        {
            null => 0,
            LinkTracking.None => nameof(LinkTracking.None).Length,
            LinkTracking.HtmlAndText => nameof(LinkTracking.HtmlAndText).Length,
            LinkTracking.HtmlOnly => nameof(LinkTracking.HtmlOnly).Length,
            LinkTracking.TextOnly => nameof(LinkTracking.TextOnly).Length,
            _ => 0,
        };
    }
}
