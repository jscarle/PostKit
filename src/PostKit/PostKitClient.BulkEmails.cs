using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Postmark.Bulk;
using PostKit.Postmark.Common;
using SendBulkEmailModel = PostKit.Postmark.Bulk.SendBulkEmailResponse;

namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<BulkEmails.SendBulkEmailResponse>> SendBulkEmailAsync(BulkEmail bulkEmail, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bulkEmail);

        var request = bulkEmail.ToBulkEmailRequest();

        Result<SendBulkEmailModel> response;
        try
        {
            response = await postmark.PostAsync<BulkEmailRequest, SendBulkEmailModel>("/email/bulk", request, cancellationToken);
        }
        catch (Exception ex)
        {
            LogBulkException(ex);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(ex);
        }

        if (response.IsFailure(out var error, out var bulkEmailStatusModel))
        {
            LogBulkError(error.Message, error);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(error);
        }

        var mappedResponse = CreateSendBulkEmailResponse(bulkEmailStatusModel);
        if (mappedResponse.IsFailure(out var mappingError, out var sendBulkEmailResponse))
        {
            LogBulkError(mappingError.Message, mappingError);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(mappingError);
        }

        LogBulkEmailSubmitted(sendBulkEmailResponse.Id, sendBulkEmailResponse.Status, sendBulkEmailResponse.TotalMessages);
        return Result.Success(sendBulkEmailResponse);
    }

    public async Task<Result<BulkEmails.SendBulkEmailResponse>> GetBulkEmailStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("The bulk request ID must not be empty.");

        Result<SendBulkEmailModel> response;
        try
        {
            response = await postmark.GetAsync<SendBulkEmailModel>($"/email/bulk/{id:D}", cancellationToken);
        }
        catch (Exception ex)
        {
            LogBulkStatusException(ex);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(ex);
        }

        if (response.IsFailure(out var error, out var bulkEmailStatusModel))
        {
            LogBulkStatusError(error.Message, error);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(error);
        }

        var mappedResponse = CreateSendBulkEmailResponse(bulkEmailStatusModel);
        if (mappedResponse.IsFailure(out var mappingError, out var sendBulkEmailResponse))
        {
            LogBulkStatusError(mappingError.Message, mappingError);
            return Result.Failure<BulkEmails.SendBulkEmailResponse>(mappingError);
        }

        return Result.Success(sendBulkEmailResponse);
    }

    private static Result<BulkEmails.SendBulkEmailResponse> CreateSendBulkEmailResponse(SendBulkEmailModel response)
    {
        if (response.Id is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("Id was not returned from the Postmark Bulk API.");

        if (!Guid.TryParse(response.Id, out var bulkRequestId))
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("Id returned from the Postmark Bulk API was not a valid GUID.");

        if (response.SubmittedAt is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("SubmittedAt was not returned from the Postmark Bulk API.");

        if (response.TotalMessages is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("TotalMessages was not returned from the Postmark Bulk API.");

        if (response.TotalMessages.Value < 0)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("TotalMessages returned from the Postmark Bulk API was invalid.");

        if (response.PercentageCompleted is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("PercentageCompleted was not returned from the Postmark Bulk API.");

        if (response.PercentageCompleted.Value is < 0 or > 100)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("PercentageCompleted returned from the Postmark Bulk API was invalid.");

        if (response.Status is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("Status was not returned from the Postmark Bulk API.");

        var status = response.Status switch
        {
            "Accepted" => BulkEmailStatus.Accepted,
            "Processing" => BulkEmailStatus.Processing,
            "Completed" => BulkEmailStatus.Completed,
            "Failed" => BulkEmailStatus.Failed,
            _ => (BulkEmailStatus?)null,
        };

        if (status is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>($"Status '{response.Status}' returned from the Postmark Bulk API is not supported.");

        if (response.Subject is null)
            return Result.Failure<BulkEmails.SendBulkEmailResponse>("Subject was not returned from the Postmark Bulk API.");

        return Result.Success(new BulkEmails.SendBulkEmailResponse(
            bulkRequestId,
            status.Value,
            response.SubmittedAt.Value,
            response.TotalMessages.Value,
            response.PercentageCompleted.Value,
            response.Subject
        ));
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to submit the bulk email request.")]
    private partial void LogBulkException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to submit the bulk email request. {Message}")]
    private partial void LogBulkError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Submitted bulk email request {BulkRequestId} with status {Status} for {TotalMessages} messages.")]
    private partial void LogBulkEmailSubmitted(Guid bulkRequestId, BulkEmailStatus status, int totalMessages);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve the bulk email request status.")]
    private partial void LogBulkStatusException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve the bulk email request status. {Message}")]
    private partial void LogBulkStatusError(string message, [LogProperties] IError error);
}
