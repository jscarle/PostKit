using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.MessageStreams;
using PostKit.Postmark;
using MessageStreamArchiveModel = PostKit.Postmark.MessageStreams.MessageStreamArchiveResponse;
using MessageStreamListModel = PostKit.Postmark.MessageStreams.MessageStreamListResponse;
using MessageStreamRequestModel = PostKit.Postmark.MessageStreams.MessageStreamRequest;
using MessageStreamResponseModel = PostKit.Postmark.MessageStreams.MessageStreamResponse;
using MessageStreamSubscriptionManagementRequestModel = PostKit.Postmark.MessageStreams.MessageStreamSubscriptionManagementRequest;
using MessageStreamSubscriptionManagementResponseModel = PostKit.Postmark.MessageStreams.MessageStreamSubscriptionManagementResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<MessageStreamList>> ListMessageStreamsAsync(MessageStreamQuery? query = null, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateMessageStreamQuery(query);
        if (validationError is not null)
            return Result.Failure<MessageStreamList>(validationError);

        Result<MessageStreamListModel> response;
        try
        {
            response = await postmark.GetAsync<MessageStreamListModel>(PostmarkTokenScope.Server, BuildMessageStreamListEndpoint(query), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListMessageStreamsException(ex);
            return Result.Failure<MessageStreamList>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListMessageStreamsError(error.Message, error);
            return Result.Failure<MessageStreamList>(error);
        }

        var mapped = CreateMessageStreamList(listModel);
        if (mapped.IsFailure(out var mappingError, out var list))
        {
            LogListMessageStreamsError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamList>(mappingError);
        }

        return Result.Success(list);
    }

    public async Task<Result<MessageStreamInfo>> GetMessageStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (streamId is null)
            throw new ArgumentNullException(nameof(streamId), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateClientMessageStreamId(streamId, "The message stream ID");
        if (validationError is not null)
            return Result.Failure<MessageStreamInfo>(validationError);

        Result<MessageStreamResponseModel> response;
        try
        {
            response = await postmark.GetAsync<MessageStreamResponseModel>(PostmarkTokenScope.Server, $"/message-streams/{Uri.EscapeDataString(streamId)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetMessageStreamException(ex);
            return Result.Failure<MessageStreamInfo>(ex);
        }

        if (response.IsFailure(out var error, out var streamModel))
        {
            LogGetMessageStreamError(error.Message, error);
            return Result.Failure<MessageStreamInfo>(error);
        }

        var mapped = CreateMessageStream(streamModel, "message stream response");
        if (mapped.IsFailure(out var mappingError, out var stream))
        {
            LogGetMessageStreamError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamInfo>(mappingError);
        }

        return Result.Success(stream);
    }

    public async Task<Result<MessageStreamInfo>> CreateMessageStreamAsync(MessageStreamCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The message stream create parameters cannot be null.");

        var mappedRequest = CreateMessageStreamRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<MessageStreamInfo>(requestError);

        Result<MessageStreamResponseModel> response;
        try
        {
            response = await postmark.PostAsync<MessageStreamRequestModel, MessageStreamResponseModel>(PostmarkTokenScope.Server, "/message-streams", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateMessageStreamException(ex);
            return Result.Failure<MessageStreamInfo>(ex);
        }

        if (response.IsFailure(out var error, out var streamModel))
        {
            LogCreateMessageStreamError(error.Message, error);
            return Result.Failure<MessageStreamInfo>(error);
        }

        var mapped = CreateMessageStream(streamModel, "message stream create response");
        if (mapped.IsFailure(out var mappingError, out var stream))
        {
            LogCreateMessageStreamError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamInfo>(mappingError);
        }

        return Result.Success(stream);
    }

    public async Task<Result<MessageStreamInfo>> EditMessageStreamAsync(string streamId, MessageStreamEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (streamId is null)
            throw new ArgumentNullException(nameof(streamId), "The message stream ID cannot be null.");

        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The message stream edit parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateClientMessageStreamId(streamId, "The message stream ID");
        if (validationError is not null)
            return Result.Failure<MessageStreamInfo>(validationError);

        var mappedRequest = CreateMessageStreamRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<MessageStreamInfo>(requestError);

        Result<MessageStreamResponseModel> response;
        try
        {
            response = await postmark.PatchAsync<MessageStreamRequestModel, MessageStreamResponseModel>(PostmarkTokenScope.Server, $"/message-streams/{Uri.EscapeDataString(streamId)}", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditMessageStreamException(ex);
            return Result.Failure<MessageStreamInfo>(ex);
        }

        if (response.IsFailure(out var error, out var streamModel))
        {
            LogEditMessageStreamError(error.Message, error);
            return Result.Failure<MessageStreamInfo>(error);
        }

        var mapped = CreateMessageStream(streamModel, "message stream edit response");
        if (mapped.IsFailure(out var mappingError, out var stream))
        {
            LogEditMessageStreamError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamInfo>(mappingError);
        }

        return Result.Success(stream);
    }

    public async Task<Result<MessageStreamArchive>> ArchiveMessageStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (streamId is null)
            throw new ArgumentNullException(nameof(streamId), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateClientMessageStreamId(streamId, "The message stream ID");
        if (validationError is not null)
            return Result.Failure<MessageStreamArchive>(validationError);

        Result<MessageStreamArchiveModel> response;
        try
        {
            response = await postmark.PostAsync<MessageStreamArchiveModel>(PostmarkTokenScope.Server, $"/message-streams/{Uri.EscapeDataString(streamId)}/archive", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogArchiveMessageStreamException(ex);
            return Result.Failure<MessageStreamArchive>(ex);
        }

        if (response.IsFailure(out var error, out var archiveModel))
        {
            LogArchiveMessageStreamError(error.Message, error);
            return Result.Failure<MessageStreamArchive>(error);
        }

        var mapped = CreateMessageStreamArchive(archiveModel);
        if (mapped.IsFailure(out var mappingError, out var archive))
        {
            LogArchiveMessageStreamError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamArchive>(mappingError);
        }

        return Result.Success(archive);
    }

    public async Task<Result<MessageStreamInfo>> UnarchiveMessageStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (streamId is null)
            throw new ArgumentNullException(nameof(streamId), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateClientMessageStreamId(streamId, "The message stream ID");
        if (validationError is not null)
            return Result.Failure<MessageStreamInfo>(validationError);

        Result<MessageStreamResponseModel> response;
        try
        {
            response = await postmark.PostAsync<MessageStreamResponseModel>(PostmarkTokenScope.Server, $"/message-streams/{Uri.EscapeDataString(streamId)}/unarchive", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogUnarchiveMessageStreamException(ex);
            return Result.Failure<MessageStreamInfo>(ex);
        }

        if (response.IsFailure(out var error, out var streamModel))
        {
            LogUnarchiveMessageStreamError(error.Message, error);
            return Result.Failure<MessageStreamInfo>(error);
        }

        var mapped = CreateMessageStream(streamModel, "message stream unarchive response");
        if (mapped.IsFailure(out var mappingError, out var stream))
        {
            LogUnarchiveMessageStreamError(mappingError.Message, mappingError);
            return Result.Failure<MessageStreamInfo>(mappingError);
        }

        return Result.Success(stream);
    }

    private static Result<MessageStreamRequestModel> CreateMessageStreamRequest(MessageStreamCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateClientMessageStreamId(parameters.Id, "The message stream create parameters ID");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateRequiredText(parameters.Name, "The message stream create parameters name");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateClearableManagementText(parameters.Description, "The message stream create parameters description");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateCreateMessageStreamType(parameters.MessageStreamType, "The message stream create parameters type");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateMessageStreamSubscriptionManagement(parameters.SubscriptionManagementConfiguration, "The message stream create parameters subscription-management configuration");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        return Result.Success(new MessageStreamRequestModel
        {
            Id = parameters.Id,
            Name = parameters.Name,
            Description = parameters.Description,
            MessageStreamType = GetMessageStreamTypeValue(parameters.MessageStreamType),
            SubscriptionManagementConfiguration = CreateMessageStreamSubscriptionManagementRequest(parameters.SubscriptionManagementConfiguration)
        });
    }

    private static Result<MessageStreamRequestModel> CreateMessageStreamRequest(MessageStreamEditParameters parameters)
    {
        if (parameters.Name is null && parameters.Description is null && parameters.SubscriptionManagementConfiguration is null)
            return Result.Failure<MessageStreamRequestModel>("The message stream edit parameters must set Name, Description, or SubscriptionManagementConfiguration.");

        var validationError = ValidationExtensions.ValidateOptionalText(parameters.Name, "The message stream edit parameters name");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateClearableManagementText(parameters.Description, "The message stream edit parameters description");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateMessageStreamSubscriptionManagement(parameters.SubscriptionManagementConfiguration, "The message stream edit parameters subscription-management configuration");
        if (validationError is not null)
            return Result.Failure<MessageStreamRequestModel>(validationError);

        return Result.Success(new MessageStreamRequestModel
        {
            Name = parameters.Name,
            Description = parameters.Description,
            SubscriptionManagementConfiguration = CreateMessageStreamSubscriptionManagementRequest(parameters.SubscriptionManagementConfiguration)
        });
    }

    private static MessageStreamSubscriptionManagementRequestModel? CreateMessageStreamSubscriptionManagementRequest(MessageStreamSubscriptionManagementConfiguration? configuration)
    {
        return configuration is null ? null : new MessageStreamSubscriptionManagementRequestModel { UnsubscribeHandlingType = GetUnsubscribeHandlingTypeValue(configuration.UnsubscribeHandlingType!.Value) };
    }

    private static string BuildMessageStreamListEndpoint(MessageStreamQuery? query)
    {
        if (query is null || (!query.MessageStreamType.HasValue && !query.IncludeArchivedStreams.HasValue))
            return "/message-streams";

        var parameters = new List<string>(2);
        if (query.MessageStreamType.HasValue)
            parameters.Add($"MessageStreamType={Uri.EscapeDataString(GetMessageStreamListTypeValue(query.MessageStreamType.Value))}");

        if (query.IncludeArchivedStreams.HasValue)
            parameters.Add($"IncludeArchivedStreams={(query.IncludeArchivedStreams.Value ? "true" : "false")}");

        return $"/message-streams?{string.Join("&", parameters)}";
    }

    private static Result<MessageStreamList> CreateMessageStreamList(MessageStreamListModel response)
    {
        if (response.MessageStreams is null)
            return Result.Failure<MessageStreamList>("MessageStreams were not returned from the Postmark Message Streams API.");

        var streams = new List<MessageStreamInfo>(response.MessageStreams.Count);
        for (var index = 0; index < response.MessageStreams.Count; index++)
        {
            var mapped = CreateMessageStream(response.MessageStreams[index], $"message stream list item {index}");
            if (mapped.IsFailure(out var error, out var stream))
                return Result.Failure<MessageStreamList>(error);

            streams.Add(stream);
        }

        return Result.Success(new MessageStreamList { MessageStreams = streams });
    }

    private static Result<MessageStreamInfo> CreateMessageStream(MessageStreamResponseModel? response, string context)
    {
        if (response is null)
            return Result.Failure<MessageStreamInfo>($"{context} returned from the Postmark Message Streams API was null.");

        if (string.IsNullOrWhiteSpace(response.Id))
            return Result.Failure<MessageStreamInfo>($"ID was not returned from the Postmark Message Streams API for {context}.");

        if (response.ServerId is null)
            return Result.Failure<MessageStreamInfo>($"ServerID was not returned from the Postmark Message Streams API for {context}.");

        if (response.ServerId.Value <= 0)
            return Result.Failure<MessageStreamInfo>($"ServerID returned from the Postmark Message Streams API for {context} was invalid. Received {response.ServerId.Value}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<MessageStreamInfo>($"Name was not returned from the Postmark Message Streams API for {context}.");

        if (string.IsNullOrWhiteSpace(response.MessageStreamType))
            return Result.Failure<MessageStreamInfo>($"MessageStreamType was not returned from the Postmark Message Streams API for {context}.");

        var type = TryMapMessageStreamType(response.MessageStreamType);
        if (type.IsFailure(out var typeError, out var mappedType))
            return Result.Failure<MessageStreamInfo>($"{context} could not be mapped: {typeError.Message}");

        if (response.CreatedAt is null)
            return Result.Failure<MessageStreamInfo>($"CreatedAt was not returned from the Postmark Message Streams API for {context}.");

        var subscriptionManagement = CreateMessageStreamSubscriptionManagement(response.SubscriptionManagementConfiguration, context);
        if (subscriptionManagement.IsFailure(out var subscriptionError, out var mappedSubscriptionManagement))
            return Result.Failure<MessageStreamInfo>(subscriptionError);

        return Result.Success(new MessageStreamInfo(response.Id, response.ServerId.Value, response.Name, ValidationExtensions.NormalizeOptionalString(response.Description), mappedType, response.CreatedAt.Value, response.UpdatedAt,
            response.ArchivedAt, response.ExpectedPurgeDate, mappedSubscriptionManagement));
    }

    private static Result<MessageStreamArchive> CreateMessageStreamArchive(MessageStreamArchiveModel response)
    {
        if (string.IsNullOrWhiteSpace(response.Id))
            return Result.Failure<MessageStreamArchive>("ID was not returned from the Postmark Message Streams API archive response.");

        if (response.ServerId is null)
            return Result.Failure<MessageStreamArchive>("ServerID was not returned from the Postmark Message Streams API archive response.");

        if (response.ServerId.Value <= 0)
            return Result.Failure<MessageStreamArchive>($"ServerID returned from the Postmark Message Streams API archive response was invalid. Received {response.ServerId.Value}.");

        if (response.ExpectedPurgeDate is null)
            return Result.Failure<MessageStreamArchive>("ExpectedPurgeDate was not returned from the Postmark Message Streams API archive response.");

        return Result.Success(new MessageStreamArchive { Id = response.Id, ServerId = response.ServerId.Value, ExpectedPurgeDate = response.ExpectedPurgeDate.Value });
    }

    private static Result<MessageStreamSubscriptionManagementConfiguration?> CreateMessageStreamSubscriptionManagement(MessageStreamSubscriptionManagementResponseModel? response, string context)
    {
        if (response is null || string.IsNullOrWhiteSpace(response.UnsubscribeHandlingType))
            return Result.Success<MessageStreamSubscriptionManagementConfiguration?>(null);

        var unsubscribeHandlingType = TryMapUnsubscribeHandlingType(response.UnsubscribeHandlingType);
        if (unsubscribeHandlingType.IsFailure(out var error, out var mapped))
            return Result.Failure<MessageStreamSubscriptionManagementConfiguration?>($"{context} could not be mapped: {error.Message}");

        return Result.Success<MessageStreamSubscriptionManagementConfiguration?>(new MessageStreamSubscriptionManagementConfiguration { UnsubscribeHandlingType = mapped });
    }

    private static string GetMessageStreamListTypeValue(MessageStreamListType type)
    {
        return type switch
        {
            MessageStreamListType.All => "all",
            MessageStreamListType.Inbound => "Inbound",
            MessageStreamListType.Transactional => "Transactional",
            MessageStreamListType.Broadcasts => "Broadcasts",
            _ => throw new NotImplementedException(
                $"Message stream list type enum value of '{nameof(MessageStreamListType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static string GetMessageStreamTypeValue(MessageStreamType type)
    {
        return type switch
        {
            MessageStreamType.Transactional => "Transactional",
            MessageStreamType.Broadcasts => "Broadcasts",
            MessageStreamType.Inbound => "Inbound",
            _ => throw new NotImplementedException(
                $"Message stream type enum value of '{nameof(MessageStreamType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<MessageStreamType> TryMapMessageStreamType(string type)
    {
        var mapped = type switch
        {
            "Inbound" => MessageStreamType.Inbound,
            "Transactional" => MessageStreamType.Transactional,
            "Broadcasts" => MessageStreamType.Broadcasts,
            _ => (MessageStreamType?)null
        };

        if (mapped is null)
            return Result.Failure<MessageStreamType>($"MessageStreamType value '{type}' returned from the Postmark Message Streams API is not supported.");

        return Result.Success(mapped.Value);
    }

    private static string GetUnsubscribeHandlingTypeValue(UnsubscribeHandlingType type)
    {
        return type switch
        {
            UnsubscribeHandlingType.None => "None",
            UnsubscribeHandlingType.Postmark => "Postmark",
            UnsubscribeHandlingType.Custom => "Custom",
            _ => throw new NotImplementedException(
                $"Unsubscribe handling type enum value of '{nameof(UnsubscribeHandlingType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<UnsubscribeHandlingType> TryMapUnsubscribeHandlingType(string type)
    {
        var mapped = type switch
        {
            "none" or "None" => UnsubscribeHandlingType.None,
            "Postmark" => UnsubscribeHandlingType.Postmark,
            "Custom" => UnsubscribeHandlingType.Custom,
            _ => (UnsubscribeHandlingType?)null
        };

        if (mapped is null)
            return Result.Failure<UnsubscribeHandlingType>($"UnsubscribeHandlingType value '{type}' returned from the Postmark Message Streams API is not supported.");

        return Result.Success(mapped.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list message streams.")]
    private partial void LogListMessageStreamsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list message streams. {Message}")]
    private partial void LogListMessageStreamsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the message stream.")]
    private partial void LogGetMessageStreamException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the message stream. {Message}")]
    private partial void LogGetMessageStreamError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the message stream.")]
    private partial void LogCreateMessageStreamException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the message stream. {Message}")]
    private partial void LogCreateMessageStreamError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the message stream.")]
    private partial void LogEditMessageStreamException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the message stream. {Message}")]
    private partial void LogEditMessageStreamError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to archive the message stream.")]
    private partial void LogArchiveMessageStreamException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to archive the message stream. {Message}")]
    private partial void LogArchiveMessageStreamError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to unarchive the message stream.")]
    private partial void LogUnarchiveMessageStreamException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to unarchive the message stream. {Message}")]
    private partial void LogUnarchiveMessageStreamError(string message, [LogProperties] IError error);
}
