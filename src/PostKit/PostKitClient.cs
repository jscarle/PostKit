using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Common;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit;

internal sealed partial class PostKitClient(IPostmarkClient postmark, ILogger<PostKitClient> logger) : IPostKitClient
{
    public async Task SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var request = email.ToEmailRequest();

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

        if (email.To is not null)
            LogEmailSent(email.To, emailResponse);
        else if (email.Cc is not null)
            LogEmailSent(email.Cc, emailResponse);
        else if (email.Bcc is not null)
            LogEmailSent(email.Bcc, emailResponse);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the email.")]
    private partial void LogException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send email. {Message}")]
    private partial void LogError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Sent email to {To}.")]
    private partial void LogEmailSent(IReadOnlyCollection<MailboxAddress> to, [LogProperties] EmailResponse emailResponse);
}
