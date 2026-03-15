using System.Collections.ObjectModel;
using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Postmark.Bulk;
using PostKit.Postmark.Common;
using SendBulkEmailModel = PostKit.Postmark.Bulk.SendBulkEmailResponse;

namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<BulkEmails.BulkEmailJob>> SendBulkEmailAsync(BulkEmail bulkEmail, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bulkEmail);

        BulkEmailRequest request;
        try
        {
            request = bulkEmail.ToBulkEmailRequest();
        }
        catch (Exception ex)
        {
            LogBulkRequestSerializationException(ex);
            return Result.Failure<BulkEmails.BulkEmailJob>(ex);
        }

        Result<SendBulkEmailModel> response;
        try
        {
            response = await postmark.PostAsync<BulkEmailRequest, SendBulkEmailModel>("/email/bulk", request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBulkException(ex);
            return Result.Failure<BulkEmails.BulkEmailJob>(ex);
        }

        if (response.IsFailure(out var error, out var bulkEmailStatusModel))
        {
            LogBulkError(error.Message, error);
            return Result.Failure<BulkEmails.BulkEmailJob>(error);
        }

        var mappedResponse = CreateBulkEmailSubmissionResponse(bulkEmailStatusModel);
        if (mappedResponse.IsFailure(out var mappingError, out var sendBulkEmailResponse))
        {
            LogBulkError(mappingError.Message, mappingError);
            return Result.Failure<BulkEmails.BulkEmailJob>(mappingError);
        }

        if (sendBulkEmailResponse.Status == BulkEmailStatus.Failed)
        {
            const string message = "The Postmark Bulk API rejected the bulk email request.";
            LogBulkRejectedStatus();
            return Result.Failure<BulkEmails.BulkEmailJob>(message);
        }

        LogBulkEmailSubmitted(sendBulkEmailResponse.Id, sendBulkEmailResponse.Status, sendBulkEmailResponse.TotalMessages);
        return Result.Success(sendBulkEmailResponse);
    }

    public async Task<Result<BulkEmails.BulkEmailJob>> GetBulkEmailStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result.Failure<BulkEmails.BulkEmailJob>("The bulk request ID must not be empty.");

        Result<SendBulkEmailModel> response;
        try
        {
            response = await postmark.GetAsync<SendBulkEmailModel>($"/email/bulk/{id:D}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBulkStatusException(ex);
            return Result.Failure<BulkEmails.BulkEmailJob>(ex);
        }

        if (response.IsFailure(out var error, out var bulkEmailStatusModel))
        {
            LogBulkStatusError(error.Message, error);
            return Result.Failure<BulkEmails.BulkEmailJob>(error);
        }

        var mappedResponse = CreateBulkEmailStatusResponse(bulkEmailStatusModel);
        if (mappedResponse.IsFailure(out var mappingError, out var sendBulkEmailResponse))
        {
            LogBulkStatusError(mappingError.Message, mappingError);
            return Result.Failure<BulkEmails.BulkEmailJob>(mappingError);
        }

        return Result.Success(sendBulkEmailResponse);
    }

    private static Result<BulkEmails.BulkEmailJob> CreateBulkEmailSubmissionResponse(SendBulkEmailModel response)
    {
        // Keep this aligned with the live API, not just the docs. Direct calls on 2026-03-13 returned
        // the same shape as the status endpoint here, including TotalMessages, PercentageCompleted, and Subject.
        return CreateBulkEmailStatusResponse(response);
    }

    private static Result<BulkEmails.BulkEmailJob> CreateBulkEmailStatusResponse(SendBulkEmailModel response)
    {
        var mappedResponse = CreateBulkEmailResponseCore(response);
        if (mappedResponse.IsFailure(out var error, out var mapped))
            return Result.Failure<BulkEmails.BulkEmailJob>(error);

        if (response.TotalMessages is null)
            return Result.Failure<BulkEmails.BulkEmailJob>("TotalMessages was not returned from the Postmark Bulk API.");

        if (response.TotalMessages.Value < 0)
            return Result.Failure<BulkEmails.BulkEmailJob>("TotalMessages returned from the Postmark Bulk API was invalid.");

        if (response.PercentageCompleted is null)
            return Result.Failure<BulkEmails.BulkEmailJob>("PercentageCompleted was not returned from the Postmark Bulk API.");

        if (response.PercentageCompleted.Value is < 0 or > 100)
            return Result.Failure<BulkEmails.BulkEmailJob>("PercentageCompleted returned from the Postmark Bulk API was invalid.");

        if (response.Subject is null)
            return Result.Failure<BulkEmails.BulkEmailJob>("Subject was not returned from the Postmark Bulk API.");

        return Result.Success(new BulkEmails.BulkEmailJob(
            mapped.Id,
            mapped.Status,
            mapped.SubmittedAt,
            response.TotalMessages.Value,
            response.PercentageCompleted.Value,
            response.Subject,
            CloneAdditionalProperties(response.AdditionalProperties)
        ));
    }

    private static Result<BulkEmailResponseCore> CreateBulkEmailResponseCore(SendBulkEmailModel response)
    {
        if (response.Id is null)
            return Result.Failure<BulkEmailResponseCore>("Id was not returned from the Postmark Bulk API.");

        if (!Guid.TryParse(response.Id, out var bulkRequestId))
            return Result.Failure<BulkEmailResponseCore>("Id returned from the Postmark Bulk API was not a valid GUID.");

        if (response.SubmittedAt is null)
            return Result.Failure<BulkEmailResponseCore>("SubmittedAt was not returned from the Postmark Bulk API.");

        if (response.Status is null)
            return Result.Failure<BulkEmailResponseCore>("Status was not returned from the Postmark Bulk API.");

        var status = response.Status switch
        {
            "Accepted" => BulkEmailStatus.Accepted,
            "Processing" => BulkEmailStatus.Processing,
            "Completed" => BulkEmailStatus.Completed,
            "Failed" => BulkEmailStatus.Failed,
            _ => (BulkEmailStatus?)null,
        };

        if (status is null)
            return Result.Failure<BulkEmailResponseCore>($"Status '{response.Status}' returned from the Postmark Bulk API is not supported.");

        return Result.Success(new BulkEmailResponseCore(bulkRequestId, status.Value, response.SubmittedAt.Value));
    }

    private static ReadOnlyDictionary<string, JsonElement>? CloneAdditionalProperties(Dictionary<string, JsonElement>? additionalProperties)
    {
        if (additionalProperties is null || additionalProperties.Count == 0)
            return null;

        return new ReadOnlyDictionary<string, JsonElement>(
            additionalProperties.ToDictionary(
                static entry => entry.Key,
                static entry => entry.Value.Clone(),
                StringComparer.Ordinal
            )
        );
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to submit the bulk email request.")]
    private partial void LogBulkException(Exception ex);

    [LoggerMessage(LogLevel.Error, "An exception occurred while serializing the bulk email request.")]
    private partial void LogBulkRequestSerializationException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to submit the bulk email request. {Message}")]
    private partial void LogBulkError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Information, "Submitted bulk email request {BulkRequestId} with status {Status} for {TotalMessages} messages.")]
    private partial void LogBulkEmailSubmitted(Guid bulkRequestId, BulkEmailStatus status, int totalMessages);

    [LoggerMessage(LogLevel.Warning, "The Postmark Bulk API returned a failed submission status.")]
    private partial void LogBulkRejectedStatus();

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve the bulk email request status.")]
    private partial void LogBulkStatusException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve the bulk email request status. {Message}")]
    private partial void LogBulkStatusError(string message, [LogProperties] IError error);

    private readonly record struct BulkEmailResponseCore(Guid Id, BulkEmailStatus Status, DateTimeOffset SubmittedAt);
}
