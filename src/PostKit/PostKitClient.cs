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
    private const int MaxBatchSize = 500;

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

    public async Task<Result<SendEmailBatchResponse>> SendEmailBatchAsync(
        IReadOnlyCollection<Email> emails,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emails);

        if (emails.Count == 0)
            return Result.Failure<SendEmailBatchResponse>("At least one email must be provided to send a batch.");

        if (emails.Count > MaxBatchSize)
        {
            return Result.Failure<SendEmailBatchResponse>(
                $"Postmark only accepts {MaxBatchSize} emails per batch request.");
        }

        var emailList = emails.ToList();
        var requests = new List<EmailRequest>(emailList.Count);
        var templateCount = 0;

        foreach (var email in emailList)
        {
            ArgumentNullException.ThrowIfNull(email);

            if (email.TemplateId.HasValue || email.TemplateAlias is not null)
                templateCount++;

            requests.Add(email.ToEmailRequest());
        }

        if (templateCount > 0 && templateCount < emailList.Count)
        {
            return Result.Failure<SendEmailBatchResponse>(
                "Each email in a batch must either use a template or none may use a template.");
        }

        var endpoint = templateCount > 0 ? "/email/batchWithTemplates" : "/email/batch";

        Result<List<EmailResponse>> response;
        try
        {
            response = await postmark.SendAsync<List<EmailRequest>, List<EmailResponse>>(endpoint, requests, cancellationToken);
        }
        catch (Exception ex)
        {
            LogBatchException(ex);
            return Result.Failure<SendEmailBatchResponse>(ex);
        }

        if (response.IsFailure(out var error, out var emailResponses))
        {
            LogBatchError(error.Message, error);
            return Result.Failure<SendEmailBatchResponse>(error);
        }

        if (emailResponses.Count != emailList.Count)
        {
            return Result.Failure<SendEmailBatchResponse>(
                "Postmark returned an unexpected number of results for the batch request.");
        }

        var batchResults = new List<SendEmailBatchResult>(emailResponses.Count);

        for (var index = 0; index < emailResponses.Count; index++)
        {
            var email = emailList[index];
            var emailResponse = emailResponses[index];

            SendEmailResponse? sendEmailResponse = null;
            if (emailResponse.MessageId is not null)
                sendEmailResponse = new SendEmailResponse(emailResponse.MessageId);

            var batchResult = new SendEmailBatchResult(
                email,
                emailResponse.Message,
                emailResponse.ErrorCode,
                emailResponse.To,
                emailResponse.SubmittedAt,
                sendEmailResponse);

            batchResults.Add(batchResult);

            if (batchResult.IsSuccessful)
            {
                if (email.To is not null)
                    LogEmailSent(email.To, emailResponse);
                else if (email.Cc is not null)
                    LogEmailSent(email.Cc, emailResponse);
                else if (email.Bcc is not null)
                    LogEmailSent(email.Bcc, emailResponse);
            }
            else
            {
                LogBatchEmailFailure(index, emailResponse.Message, emailResponse.ErrorCode);
            }
        }

        var sendEmailBatchResponse = new SendEmailBatchResponse(batchResults.AsReadOnly());

        return Result.Success(sendEmailBatchResponse);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the email.")]
    private partial void LogException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send email. {Message}")]
    private partial void LogError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Sent email to {To}.")]
    private partial void LogEmailSent(IReadOnlyCollection<MailboxAddress> to, [LogProperties] EmailResponse emailResponse);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the batch of emails.")]
    private partial void LogBatchException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send the batch of emails. {Message}")]
    private partial void LogBatchError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Warning, "Postmark rejected email {Index} in the batch. {Message} (ErrorCode: {ErrorCode})")]
    private partial void LogBatchEmailFailure(int index, string message, int errorCode);
}
