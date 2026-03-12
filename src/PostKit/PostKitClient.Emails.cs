using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Emails;
using PostKit.Errors;
using PostKit.Postmark.Common;
using PostKit.Postmark.Email;

namespace PostKit;

internal sealed partial class PostKitClient
{
    private const int MaxBatchSize = 500;

    public async Task<Result<EmailSubmission>> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        EmailRequest request;
        try
        {
            request = email.ToEmailRequest();
        }
        catch (Exception ex)
        {
            LogRequestSerializationException(ex);
            return Result.Failure<EmailSubmission>(ex);
        }

        var endpoint = email.TemplateId.HasValue || email.TemplateAlias is not null ? "/email/withTemplate" : "/email";

        Result<EmailResponse> response;
        try
        {
            response = await postmark.PostAsync<EmailRequest, EmailResponse>(endpoint, request, cancellationToken);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Result.Failure<EmailSubmission>(ex);
        }

        if (response.IsFailure(out var error, out var emailResponse))
        {
            LogError(error.Message, error);
            return Result.Failure<EmailSubmission>(error);
        }

        if (email.To is not null)
            LogEmailSent(email.To, emailResponse);
        else if (email.Cc is not null)
            LogEmailSent(email.Cc, emailResponse);
        else if (email.Bcc is not null)
            LogEmailSent(email.Bcc, emailResponse);

        if (emailResponse.MessageId is null)
            return Result.Failure<EmailSubmission>("Message ID was not returned from the Postmark API.");

        if (!Guid.TryParse(emailResponse.MessageId, out var parsedMessageId))
            return Result.Failure<EmailSubmission>("Message ID returned from the Postmark API was not a valid GUID.");

        if (emailResponse.SubmittedAt is null)
            return Result.Failure<EmailSubmission>("SubmittedAt was not returned from the Postmark API.");

        var sendEmailResponse = new EmailSubmission(parsedMessageId, emailResponse.To, emailResponse.SubmittedAt.Value);

        return Result.Success(sendEmailResponse);
    }

    public async Task<Result<EmailBatchSubmission>> SendEmailBatchAsync(IReadOnlyCollection<Email> emails, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emails);

        if (emails.Count == 0)
            return Result.Failure<EmailBatchSubmission>("At least one email must be provided to send a batch.");

        if (emails.Count > MaxBatchSize)
            return Result.Failure<EmailBatchSubmission>($"Postmark only accepts {MaxBatchSize} emails per batch request.");

        var emailList = emails.ToList();
        var requests = new List<EmailRequest>(emailList.Count);
        var templateCount = 0;
        long estimatedBatchSize = 0;

        foreach (var email in emailList)
        {
            ArgumentNullException.ThrowIfNull(email);

            if (email.TemplateId.HasValue || email.TemplateAlias is not null)
                templateCount++;

            estimatedBatchSize += PostmarkSizeEstimator.EstimateMessageSizeLowerBound(email);
            try
            {
                requests.Add(email.ToEmailRequest());
            }
            catch (Exception ex)
            {
                LogBatchRequestSerializationException(index: requests.Count, ex);
                return Result.Failure<EmailBatchSubmission>(ex);
            }
        }

        if (templateCount > 0 && templateCount < emailList.Count)
            return Result.Failure<EmailBatchSubmission>("Each email in a batch must either use a template or none may use a template.");

        var sendWithTemplates = templateCount > 0;
        var endpoint = sendWithTemplates ? "/email/batchWithTemplates" : "/email/batch";

        if (estimatedBatchSize > PostmarkSizeEstimator.BatchPayloadSizeLimitInBytes)
            return Result.Failure<EmailBatchSubmission>("Batch payload size exceeds Postmark's 50 MB limit.");

        Result<List<EmailResponse>> response;
        try
        {
            if (sendWithTemplates)
            {
                var request = new EmailBatchRequest { Messages = requests };
                response = await postmark.PostAsync<EmailBatchRequest, List<EmailResponse>>(endpoint, request, cancellationToken);
            }
            else
            {
                response = await postmark.PostAsync<List<EmailRequest>, List<EmailResponse>>(endpoint, requests, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            LogBatchException(ex);
            return Result.Failure<EmailBatchSubmission>(ex);
        }

        if (response.IsFailure(out var error, out var emailResponses))
        {
            LogBatchError(error.Message, error);
            return Result.Failure<EmailBatchSubmission>(error);
        }

        if (emailResponses.Count != emailList.Count)
            return Result.Failure<EmailBatchSubmission>("Postmark returned an unexpected number of results for the batch request.");

        var batchResults = new List<Result<EmailSubmission>>(emailResponses.Count);

        for (var index = 0; index < emailResponses.Count; index++)
        {
            var email = emailList[index];
            var emailResponse = emailResponses[index];

            if (emailResponse.ErrorCode == 0)
            {
                if (emailResponse.MessageId is null)
                    return Result.Failure<EmailBatchSubmission>($"Message ID was not returned from the Postmark API for batch item {index}.");

                if (!Guid.TryParse(emailResponse.MessageId, out var parsedMessageId))
                    return Result.Failure<EmailBatchSubmission>($"Message ID returned from the Postmark API for batch item {index} was not a valid GUID.");

                if (emailResponse.SubmittedAt is null)
                    return Result.Failure<EmailBatchSubmission>($"SubmittedAt was not returned from the Postmark API for batch item {index}.");

                if (email.To is not null)
                    LogEmailSent(email.To, emailResponse);
                else if (email.Cc is not null)
                    LogEmailSent(email.Cc, emailResponse);
                else if (email.Bcc is not null)
                    LogEmailSent(email.Bcc, emailResponse);

                var sendEmailResponse = new EmailSubmission(parsedMessageId, emailResponse.To, emailResponse.SubmittedAt.Value);
                batchResults.Add(Result.Success(sendEmailResponse));
            }
            else
            {
                LogBatchEmailFailure(index, emailResponse.Message, emailResponse.ErrorCode);
                batchResults.Add(Result.Failure<EmailSubmission>(new PostmarkError(emailResponse.ErrorCode, emailResponse.Message)));
            }
        }

        var sendEmailBatchResponse = new EmailBatchSubmission(batchResults.AsReadOnly());

        return Result.Success(sendEmailBatchResponse);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the email.")]
    private partial void LogException(Exception ex);

    [LoggerMessage(LogLevel.Error, "An exception occurred while serializing the email request.")]
    private partial void LogRequestSerializationException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send email. {Message}")]
    private partial void LogError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Sent email to {To}.")]
    private partial void LogEmailSent(IReadOnlyCollection<MailboxAddress> to, [LogProperties] EmailResponse emailResponse);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to send the batch of emails.")]
    private partial void LogBatchException(Exception ex);

    [LoggerMessage(LogLevel.Error, "An exception occurred while serializing email {Index} for the batch request.")]
    private partial void LogBatchRequestSerializationException(int index, Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to send the batch of emails. {Message}")]
    private partial void LogBatchError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Warning, "Postmark rejected email {Index} in the batch. {Message} (ErrorCode: {ErrorCode})")]
    private partial void LogBatchEmailFailure(int index, string message, int errorCode);
}
