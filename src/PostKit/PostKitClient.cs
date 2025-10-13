using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Common;
using PostKit.Postmark;
using PostKit.Postmark.Email;
using PostKit.Responses;

namespace PostKit;

internal sealed partial class PostKitClient(IPostmarkClient postmark, ILogger<PostKitClient> logger) : IPostKitClient
{
    public async Task<Result<SendEmailResponse>> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var request = email.ToEmailRequest();

        var endpoint = email.TemplateId.HasValue || email.TemplateAlias is not null ? "/email/withTemplate" : "/email";

        Result<EmailResponse> response;
        try
        {
            response = await postmark.SendAsync<EmailRequest, EmailResponse>(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Result.Failure<SendEmailResponse>(ex);
        }

        if (response.IsFailure(out var error, out var emailResponse))
        {
            LogError(error.Message, error);
            return Result.Failure<SendEmailResponse>(error);
        }

        if (email.To is not null)
            LogEmailSent(email.To, emailResponse);
        else if (email.Cc is not null)
            LogEmailSent(email.Cc, emailResponse);
        else if (email.Bcc is not null)
            LogEmailSent(email.Bcc, emailResponse);

        if (emailResponse.MessageId is null)
            return Result.Failure<SendEmailResponse>("Message ID was not returned from the Postmark API.");

        var sendEmailResponse = new SendEmailResponse(emailResponse.MessageId);

        return Result.Success(sendEmailResponse);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the email.")]
    private partial void LogException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send email. {Message}")]
    private partial void LogError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Sent email to {To}.")]
    private partial void LogEmailSent(IReadOnlyCollection<MailboxAddress> to, [LogProperties] EmailResponse emailResponse);
}
