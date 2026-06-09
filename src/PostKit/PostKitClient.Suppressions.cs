using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Suppressions;
using SuppressionBatchModel = PostKit.Postmark.Suppressions.SuppressionBatchResponse;
using SuppressionBatchRequestModel = PostKit.Postmark.Suppressions.SuppressionBatchRequest;
using SuppressionDumpItemModel = PostKit.Postmark.Suppressions.SuppressionDumpItemResponse;
using SuppressionDumpModel = PostKit.Postmark.Suppressions.SuppressionDumpResponse;
using SuppressionRequestItemModel = PostKit.Postmark.Suppressions.SuppressionRequestItem;
using SuppressionResultModel = PostKit.Postmark.Suppressions.SuppressionResultResponse;

namespace PostKit;

internal sealed partial class PostKitClient
{
    private const int MaxSuppressionBatchSize = 50;

    public Task<Result<SuppressionDump>> GetSuppressionsAsync(MessageStream messageStream, CancellationToken cancellationToken = default)
    {
        return GetSuppressionsAsync(messageStream, null, cancellationToken);
    }

    public Task<Result<SuppressionDump>> GetSuppressionsAsync(string messageStream, CancellationToken cancellationToken = default)
    {
        return GetSuppressionsAsync(messageStream, null, cancellationToken);
    }

    public Task<Result<SuppressionDump>> GetSuppressionsAsync(MessageStream messageStream, SuppressionQuery? query, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The suppression query message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<SuppressionDump>(error));

        return GetSuppressionsAsync(messageStreamId, query, cancellationToken);
    }

    public async Task<Result<SuppressionDump>> GetSuppressionsAsync(string messageStream, SuppressionQuery? query, CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidateSuppressionMessageStream(messageStream, "The suppression query message stream")
            ?? ValidateSuppressionQuery(query);
        if (validationError is not null)
            return Result.Failure<SuppressionDump>(validationError);

        Result<SuppressionDumpModel> response;
        try
        {
            var endpoint = BuildSuppressionDumpEndpoint(messageStream, query);
            response = await postmark.GetAsync<SuppressionDumpModel>(endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogSuppressionsException(ex);
            return Result.Failure<SuppressionDump>(ex);
        }

        if (response.IsFailure(out var error, out var suppressionDumpModel))
        {
            LogSuppressionsError(error.Message, error);
            return Result.Failure<SuppressionDump>(error);
        }

        var mappedResponse = CreateSuppressionDumpResponse(suppressionDumpModel);
        if (mappedResponse.IsFailure(out var mappingError, out var suppressionDump))
        {
            LogSuppressionsError(mappingError.Message, mappingError);
            return Result.Failure<SuppressionDump>(mappingError);
        }

        return Result.Success(suppressionDump);
    }

    public Task<Result<SuppressionBatch>> CreateSuppressionsAsync(MessageStream messageStream, string emailAddress, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The suppression create message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<SuppressionBatch>(error));

        return CreateSuppressionsAsync(messageStreamId, emailAddress, cancellationToken);
    }

    public Task<Result<SuppressionBatch>> CreateSuppressionsAsync(string messageStream, string emailAddress, CancellationToken cancellationToken = default)
    {
        return CreateSuppressionsAsync(messageStream, [emailAddress], cancellationToken);
    }

    public Task<Result<SuppressionBatch>> CreateSuppressionsAsync(MessageStream messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The suppression create message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<SuppressionBatch>(error));

        return CreateSuppressionsAsync(messageStreamId, emailAddresses, cancellationToken);
    }

    public Task<Result<SuppressionBatch>> CreateSuppressionsAsync(string messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default)
    {
        return ChangeSuppressionsAsync(messageStream, emailAddresses, "/suppressions", "create", LogCreateSuppressionsException, LogCreateSuppressionsError, cancellationToken);
    }

    public Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(MessageStream messageStream, string emailAddress, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The suppression delete message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<SuppressionBatch>(error));

        return DeleteSuppressionsAsync(messageStreamId, emailAddress, cancellationToken);
    }

    public Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(string messageStream, string emailAddress, CancellationToken cancellationToken = default)
    {
        return DeleteSuppressionsAsync(messageStream, [emailAddress], cancellationToken);
    }

    public Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(MessageStream messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The suppression delete message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<SuppressionBatch>(error));

        return DeleteSuppressionsAsync(messageStreamId, emailAddresses, cancellationToken);
    }

    public Task<Result<SuppressionBatch>> DeleteSuppressionsAsync(string messageStream, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default)
    {
        return ChangeSuppressionsAsync(messageStream, emailAddresses, "/suppressions/delete", "delete", LogDeleteSuppressionsException, LogDeleteSuppressionsError, cancellationToken);
    }

    private async Task<Result<SuppressionBatch>> ChangeSuppressionsAsync(
        string messageStream,
        IEnumerable<string> emailAddresses,
        string endpointSuffix,
        string operationName,
        Action<Exception> logException,
        Action<string, IError> logError,
        CancellationToken cancellationToken
    )
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        if (emailAddresses is null)
            throw new ArgumentNullException(nameof(emailAddresses), "The suppression email address collection cannot be null.");

        var validationError = ValidateSuppressionMessageStream(messageStream, $"The suppression {operationName} message stream");
        if (validationError is not null)
            return Result.Failure<SuppressionBatch>(validationError);

        var mappedRequest = CreateSuppressionBatchRequest(emailAddresses, operationName);
        if (mappedRequest.IsFailure(out var requestError, out var request))
            return Result.Failure<SuppressionBatch>(requestError);

        Result<SuppressionBatchModel> response;
        try
        {
            var endpoint = $"/message-streams/{Uri.EscapeDataString(messageStream)}{endpointSuffix}";
            response = await postmark.PostAsync<SuppressionBatchRequestModel, SuppressionBatchModel>(endpoint, request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logException(ex);
            return Result.Failure<SuppressionBatch>(ex);
        }

        if (response.IsFailure(out var error, out var suppressionBatchModel))
        {
            logError(error.Message, error);
            return Result.Failure<SuppressionBatch>(error);
        }

        var mappedResponse = CreateSuppressionBatchResponse(suppressionBatchModel);
        if (mappedResponse.IsFailure(out var mappingError, out var suppressionBatch))
        {
            logError(mappingError.Message, mappingError);
            return Result.Failure<SuppressionBatch>(mappingError);
        }

        return Result.Success(suppressionBatch);
    }

    private static string? ValidateSuppressionQuery(SuppressionQuery? query)
    {
        if (query is null)
            return null;

        if (query.EmailAddress is not null && string.IsNullOrWhiteSpace(query.EmailAddress))
            return FormatEmptySuppressionQueryFilterMessage("The suppression query email address filter", nameof(SuppressionQuery.EmailAddress), query.EmailAddress);

        if (query is { FromDate: not null, ToDate: not null } && query.FromDate.Value > query.ToDate.Value)
            return $"The suppression query from-date must not be later than the to-date. FromDate: {FormatSuppressionQueryDate(query.FromDate.Value)}; ToDate: {FormatSuppressionQueryDate(query.ToDate.Value)}.";

        return null;
    }

    private static Result<string> GetMessageStreamId(MessageStream messageStream, string subject)
    {
        return messageStream switch
        {
            MessageStream.Transactional => Result.Success("outbound"),
            MessageStream.Broadcast => Result.Success("broadcast"),
            _ => Result.Failure<string>($"{subject} must be MessageStream.Transactional or MessageStream.Broadcast. Received {(int)messageStream}."),
        };
    }

    private static string? ValidateSuppressionMessageStream(string? messageStream, string subject)
    {
        if (string.IsNullOrWhiteSpace(messageStream))
            return $"{subject} cannot be empty or whitespace. Actual length: {messageStream?.Length ?? 0}.";

        if (!messageStream.AsSpan()
                .IsValidMessageStreamId())
            return ValidationExtensions.FormatMessageStreamIdValidationMessage(messageStream, subject);

        return null;
    }

    private static string FormatEmptySuppressionQueryFilterMessage(string subject, string propertyName, string? value)
    {
        return $"{subject} cannot be empty or whitespace. Set {propertyName} to null to omit this filter. Actual length: {value?.Length ?? 0}.";
    }

    private static Result<SuppressionBatchRequestModel> CreateSuppressionBatchRequest(IEnumerable<string> emailAddresses, string operationName)
    {
        var addresses = emailAddresses.ToList();
        if (addresses.Count is < 1 or > MaxSuppressionBatchSize)
            return Result.Failure<SuppressionBatchRequestModel>($"The suppression {operationName} request email address collection must contain between 1 and {MaxSuppressionBatchSize} addresses. Actual count: {addresses.Count}.");

        var requestItems = new List<SuppressionRequestItemModel>(addresses.Count);
        for (var index = 0; index < addresses.Count; index++)
        {
            var emailAddress = addresses[index];
            if (string.IsNullOrWhiteSpace(emailAddress))
                return Result.Failure<SuppressionBatchRequestModel>($"The suppression {operationName} request email address at index {index} cannot be empty or whitespace. Actual length: {emailAddress.Length}.");

            requestItems.Add(new SuppressionRequestItemModel { EmailAddress = emailAddress });
        }

        return Result.Success(new SuppressionBatchRequestModel { Suppressions = requestItems });
    }

    private static string BuildSuppressionDumpEndpoint(string messageStream, SuppressionQuery? query)
    {
        var endpoint = $"/message-streams/{Uri.EscapeDataString(messageStream)}/suppressions/dump";
        var parameters = new List<string>(5);

        if (query is null)
            return endpoint;

        if (query.Reason.HasValue)
            parameters.Add($"SuppressionReason={Uri.EscapeDataString(GetSuppressionReasonValue(query.Reason.Value))}");

        if (query.Origin.HasValue)
            parameters.Add($"Origin={Uri.EscapeDataString(GetSuppressionOriginValue(query.Origin.Value))}");

        if (query.ToDate.HasValue)
            parameters.Add($"todate={Uri.EscapeDataString(FormatSuppressionQueryDate(query.ToDate.Value))}");

        if (query.FromDate.HasValue)
            parameters.Add($"fromdate={Uri.EscapeDataString(FormatSuppressionQueryDate(query.FromDate.Value))}");

        if (query.EmailAddress is not null)
            parameters.Add($"EmailAddress={Uri.EscapeDataString(query.EmailAddress)}");

        return parameters.Count == 0 ? endpoint : $"{endpoint}?{string.Join("&", parameters)}";
    }

    private static string FormatSuppressionQueryDate(DateOnly value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static Result<SuppressionDump> CreateSuppressionDumpResponse(SuppressionDumpModel response)
    {
        if (response.Suppressions is null)
            return Result.Failure<SuppressionDump>("Suppressions were not returned from the Postmark Suppressions API.");

        var suppressions = new List<Suppression>(response.Suppressions.Count);
        for (var index = 0; index < response.Suppressions.Count; index++)
        {
            var mappedSuppression = CreateSuppression(response.Suppressions[index]);
            if (mappedSuppression.IsFailure(out var error, out var suppression))
                return Result.Failure<SuppressionDump>($"Suppression item {index} could not be mapped: {error.Message}");

            suppressions.Add(suppression);
        }

        return Result.Success(new SuppressionDump(suppressions));
    }

    private static Result<Suppression> CreateSuppression(SuppressionDumpItemModel response)
    {
        if (string.IsNullOrWhiteSpace(response.EmailAddress))
            return Result.Failure<Suppression>("EmailAddress was not returned from the Postmark Suppressions API.");

        if (string.IsNullOrWhiteSpace(response.SuppressionReason))
            return Result.Failure<Suppression>("SuppressionReason was not returned from the Postmark Suppressions API.");

        var mappedReason = TryMapSuppressionReason(response.SuppressionReason);
        if (mappedReason.IsFailure(out var reasonError, out var reason))
            return Result.Failure<Suppression>(reasonError);

        if (string.IsNullOrWhiteSpace(response.Origin))
            return Result.Failure<Suppression>("Origin was not returned from the Postmark Suppressions API.");

        var mappedOrigin = TryMapSuppressionOrigin(response.Origin);
        if (mappedOrigin.IsFailure(out var originError, out var origin))
            return Result.Failure<Suppression>(originError);

        if (response.CreatedAt is null)
            return Result.Failure<Suppression>("CreatedAt was not returned from the Postmark Suppressions API.");

        return Result.Success(new Suppression(response.EmailAddress, reason, origin, response.CreatedAt.Value));
    }

    private static Result<SuppressionBatch> CreateSuppressionBatchResponse(SuppressionBatchModel response)
    {
        if (response.Suppressions is null)
            return Result.Failure<SuppressionBatch>("Suppressions were not returned from the Postmark Suppressions API.");

        var suppressions = new List<SuppressionResult>(response.Suppressions.Count);
        for (var index = 0; index < response.Suppressions.Count; index++)
        {
            var mappedSuppression = CreateSuppressionResult(response.Suppressions[index]);
            if (mappedSuppression.IsFailure(out var error, out var suppression))
                return Result.Failure<SuppressionBatch>($"Suppression result item {index} could not be mapped: {error.Message}");

            suppressions.Add(suppression);
        }

        return Result.Success(new SuppressionBatch(suppressions));
    }

    private static Result<SuppressionResult> CreateSuppressionResult(SuppressionResultModel response)
    {
        if (string.IsNullOrWhiteSpace(response.EmailAddress))
            return Result.Failure<SuppressionResult>("EmailAddress was not returned from the Postmark Suppressions API.");

        if (string.IsNullOrWhiteSpace(response.Status))
            return Result.Failure<SuppressionResult>("Status was not returned from the Postmark Suppressions API.");

        var mappedStatus = TryMapSuppressionStatus(response.Status);
        if (mappedStatus.IsFailure(out var statusError, out var status))
            return Result.Failure<SuppressionResult>(statusError);

        return Result.Success(new SuppressionResult(response.EmailAddress, status, response.Message));
    }

    private static string GetSuppressionReasonValue(SuppressionReason reason)
    {
        return reason switch
        {
            SuppressionReason.HardBounce => "HardBounce",
            SuppressionReason.SpamComplaint => "SpamComplaint",
            SuppressionReason.ManualSuppression => "ManualSuppression",
            _ => throw new NotImplementedException($"Suppression reason enum value of '{nameof(SuppressionReason)}.{reason}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues)."),
        };
    }

    private static Result<SuppressionReason> TryMapSuppressionReason(string reason)
    {
        var mappedReason = reason switch
        {
            "HardBounce" => SuppressionReason.HardBounce,
            "SpamComplaint" => SuppressionReason.SpamComplaint,
            "ManualSuppression" => SuppressionReason.ManualSuppression,
            _ => (SuppressionReason?)null,
        };

        if (mappedReason is null)
            return Result.Failure<SuppressionReason>($"SuppressionReason value '{reason}' returned from the Postmark Suppressions API is not supported.");

        return Result.Success(mappedReason.Value);
    }

    private static string GetSuppressionOriginValue(SuppressionOrigin origin)
    {
        return origin switch
        {
            SuppressionOrigin.Recipient => "Recipient",
            SuppressionOrigin.Customer => "Customer",
            SuppressionOrigin.Admin => "Admin",
            _ => throw new NotImplementedException($"Suppression origin enum value of '{nameof(SuppressionOrigin)}.{origin}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues)."),
        };
    }

    private static Result<SuppressionOrigin> TryMapSuppressionOrigin(string origin)
    {
        var mappedOrigin = origin switch
        {
            "Recipient" => SuppressionOrigin.Recipient,
            "Customer" => SuppressionOrigin.Customer,
            "Admin" => SuppressionOrigin.Admin,
            _ => (SuppressionOrigin?)null,
        };

        if (mappedOrigin is null)
            return Result.Failure<SuppressionOrigin>($"Origin value '{origin}' returned from the Postmark Suppressions API is not supported.");

        return Result.Success(mappedOrigin.Value);
    }

    private static Result<SuppressionStatus> TryMapSuppressionStatus(string status)
    {
        var mappedStatus = status switch
        {
            "Failed" => SuppressionStatus.Failed,
            "Suppressed" => SuppressionStatus.Suppressed,
            "Deleted" => SuppressionStatus.Deleted,
            _ => (SuppressionStatus?)null,
        };

        if (mappedStatus is null)
            return Result.Failure<SuppressionStatus>($"Status value '{status}' returned from the Postmark Suppressions API is not supported.");

        return Result.Success(mappedStatus.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve suppressions.")]
    private partial void LogSuppressionsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve suppressions. {Message}")]
    private partial void LogSuppressionsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create suppressions.")]
    private partial void LogCreateSuppressionsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create suppressions. {Message}")]
    private partial void LogCreateSuppressionsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete suppressions.")]
    private partial void LogDeleteSuppressionsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete suppressions. {Message}")]
    private partial void LogDeleteSuppressionsError(string message, [LogProperties] IError error);
}
