using LightResults;
using Microsoft.Extensions.Logging;
using MimeKit;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.Postmark.Email;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    private const int MaxBatchSize = 500;

    public async Task<Result<EmailSubmission>> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email), "The email cannot be null.");

        EmailRequest request;
        try
        {
            request = email.ToEmailRequest();
        }
        catch (Exception ex)
        {
            LogRequestSerializationException(ex);
            return Result.Failure<EmailSubmission>($"The email could not be prepared for sending: {ex.Message}");
        }

        var endpoint = email.TemplateId.HasValue || email.TemplateAlias is not null ? "/email/withTemplate" : "/email";

        Result<EmailResponse> response;
        try
        {
            response = await postmark.PostAsync<EmailRequest, EmailResponse>(PostmarkTokenScope.Server, endpoint, request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
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
        {
            var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, "The successful response did not include the accepted message identifier.");
            return Result.Failure<EmailSubmission>(invalidResponseError);
        }

        if (!Guid.TryParse(emailResponse.MessageId, out var parsedMessageId))
        {
            var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, $"The successful response contained an invalid accepted message identifier: '{emailResponse.MessageId}'.");
            return Result.Failure<EmailSubmission>(invalidResponseError);
        }

        if (emailResponse.SubmittedAt is null)
        {
            var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, "The successful response did not include the accepted submission time.");
            return Result.Failure<EmailSubmission>(invalidResponseError);
        }

        var internetMessageId = EmailSubmission.ResolveInternetMessageId(parsedMessageId, email.Headers, true);
        var sendEmailResponse = new EmailSubmission(parsedMessageId, emailResponse.To, emailResponse.SubmittedAt.Value, internetMessageId);

        return Result.Success(sendEmailResponse);
    }

    public async Task<Result<EmailBatchSubmission>> SendEmailBatchAsync(IEnumerable<Email> emails, CancellationToken cancellationToken = default)
    {
        if (emails is null)
            throw new ArgumentNullException(nameof(emails), "The email batch cannot be null.");

        var emailList = emails.ToList();

        if (emailList.Count == 0)
            return Result.Failure<EmailBatchSubmission>("At least one email must be provided to send a batch.");

        if (emailList.Count > MaxBatchSize)
            return Result.Failure<EmailBatchSubmission>($"Postmark only accepts {MaxBatchSize} emails per batch request. Received {emailList.Count}.");

        var requests = new List<EmailRequest>(emailList.Count);
        var templateCount = 0;
        int? firstTemplatedIndex = null;
        int? firstNonTemplatedIndex = null;
        long estimatedBatchSize = 0;

        for (var index = 0; index < emailList.Count; index++)
        {
            var email = emailList[index];
            if (email is null)
                throw new ArgumentException($"The email at index {index} cannot be null.", nameof(emails));

            var usesTemplate = email.TemplateId.HasValue || email.TemplateAlias is not null;
            if (usesTemplate)
            {
                templateCount++;
                firstTemplatedIndex ??= index;
            }
            else
            {
                firstNonTemplatedIndex ??= index;
            }

            estimatedBatchSize += PostmarkSizeEstimator.EstimateMessagePayloadSizeLowerBound(email);
            try
            {
                requests.Add(email.ToEmailRequest());
            }
            catch (Exception ex)
            {
                LogBatchRequestSerializationException(index, ex);
                return Result.Failure<EmailBatchSubmission>($"Batch item {index} could not be prepared for sending: {ex.Message}");
            }
        }

        if (templateCount > 0 && templateCount < emailList.Count)
            return Result.Failure<EmailBatchSubmission>(
                $"Each email in a batch must either use a template or none may use a template. Found {templateCount} templated and {emailList.Count - templateCount} non-templated emails; first templated item index: {firstTemplatedIndex}, first non-templated item index: {firstNonTemplatedIndex}.");

        var sendWithTemplates = templateCount > 0;
        var endpoint = sendWithTemplates ? "/email/batchWithTemplates" : "/email/batch";

        if (estimatedBatchSize > PostmarkSizeEstimator.BatchPayloadSizeLimitInBytes)
            return Result.Failure<EmailBatchSubmission>(PostmarkSizeEstimator.FormatEstimatedSizeLimitMessage("Estimated batch payload size exceeds Postmark's 50 MB limit.", estimatedBatchSize,
                PostmarkSizeEstimator.BatchPayloadSizeLimitInBytes));

        Result<List<EmailResponse>> response;
        try
        {
            if (sendWithTemplates)
            {
                var request = new EmailBatchRequest { Messages = requests };
                response = await postmark.PostAsync<EmailBatchRequest, List<EmailResponse>>(PostmarkTokenScope.Server, endpoint, request, cancellationToken);
            }
            else
            {
                response = await postmark.PostAsync<List<EmailRequest>, List<EmailResponse>>(PostmarkTokenScope.Server, endpoint, requests, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
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
        {
            var details = $"The successful response contained an unexpected number of batch results. Expected {emailList.Count}, received {emailResponses.Count}.";
            var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, details);
            return Result.Failure<EmailBatchSubmission>(invalidResponseError);
        }

        var batchResults = new List<Result<EmailSubmission>>(emailResponses.Count);

        for (var index = 0; index < emailResponses.Count; index++)
        {
            var email = emailList[index];
            var emailResponse = emailResponses[index];

            if (emailResponse.ErrorCode == 0)
            {
                if (emailResponse.MessageId is null)
                {
                    var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, $"The successful response did not include the accepted message identifier for batch item {index}.");
                    return Result.Failure<EmailBatchSubmission>(invalidResponseError);
                }

                if (!Guid.TryParse(emailResponse.MessageId, out var parsedMessageId))
                {
                    var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint,
                        $"The successful response contained an invalid accepted message identifier for batch item {index}: '{emailResponse.MessageId}'.");
                    return Result.Failure<EmailBatchSubmission>(invalidResponseError);
                }

                if (emailResponse.SubmittedAt is null)
                {
                    var invalidResponseError = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, $"The successful response did not include the accepted submission time for batch item {index}.");
                    return Result.Failure<EmailBatchSubmission>(invalidResponseError);
                }

                if (email.To is not null)
                    LogEmailSent(email.To, emailResponse);
                else if (email.Cc is not null)
                    LogEmailSent(email.Cc, emailResponse);
                else if (email.Bcc is not null)
                    LogEmailSent(email.Bcc, emailResponse);

                var internetMessageId = EmailSubmission.ResolveInternetMessageId(parsedMessageId, email.Headers, true);
                var sendEmailResponse = new EmailSubmission(parsedMessageId, emailResponse.To, emailResponse.SubmittedAt.Value, internetMessageId);
                batchResults.Add(Result.Success(sendEmailResponse));
            }
            else
            {
                LogBatchEmailFailure(index, emailResponse.Message, emailResponse.ErrorCode);
                var postmarkError = new PostmarkError(emailResponse.ErrorCode, emailResponse.Message);
                var failure = Result.Failure<EmailSubmission>(postmarkError);
                batchResults.Add(failure);
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
