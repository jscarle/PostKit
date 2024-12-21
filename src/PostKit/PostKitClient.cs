using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit;

internal sealed partial class PostKitClient(IPostmarkClient postmark, ILogger<PostKitClient> logger) : IPostKitClient
{
    public async Task SendEmailAsync(Email email, MessageStream messageStream = MessageStream.Transactional, CancellationToken cancellationToken = default)
    {
        var from = email.From.ToString(true);
        var replyTo = email.ReplyTo is not null ? string.Join(",", email.ReplyTo.Select(x => x.ToString(true))) : null;
        var to = string.Join(",", email.To.Select(x => x.GetAddress(true)));
        var cc = email.Cc is not null ? string.Join(",", email.Cc.Select(x => x.GetAddress(true))) : null;
        var bcc = email.Bcc is not null ? string.Join(",", email.Bcc.Select(x => x.GetAddress(true))) : null;

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
            MessageStream = messageStream == MessageStream.Broadcast ? "broadcast" : "outbound",
        };

        Result<EmailResponse> response;
        try
        {
            response = await postmark.SendAsync<EmailRequest, EmailResponse>("/email", request, cancellationToken);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return;
        }

        if (response.IsFailure(out var error, out var emailResponse))
        {
            LogError(error.Message, error);
            return;
        }

        LogEmailSent(email.To, emailResponse);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the email.")]
    private partial void LogException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send email. {Message}")]
    private partial void LogError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Sent email to {To}.")]
    private partial void LogEmailSent(IReadOnlyCollection<MailboxAddress> to, [LogProperties] EmailResponse emailResponse);
}
