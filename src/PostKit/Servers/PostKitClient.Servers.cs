using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Servers;
using ServerDeletionModel = PostKit.Postmark.Servers.ServerDeletionResponse;
using ServerListModel = PostKit.Postmark.Servers.ServerListResponse;
using ServerRequestModel = PostKit.Postmark.Servers.ServerRequest;
using ServerResponseModel = PostKit.Postmark.Servers.ServerResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<PostmarkServer>> GetServerAsync(CancellationToken cancellationToken = default)
    {
        Result<ServerResponseModel> response;
        try
        {
            response = await postmark.GetAsync<ServerResponseModel>(PostmarkTokenScope.Server, "/server", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetServerException(ex);
            return Result.Failure<PostmarkServer>(ex);
        }

        if (response.IsFailure(out var error, out var serverModel))
        {
            LogGetServerError(error.Message, error);
            return Result.Failure<PostmarkServer>(error);
        }

        var mapped = CreatePostmarkServer(serverModel, "server response");
        if (mapped.IsFailure(out var mappingError, out var server))
        {
            LogGetServerError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkServer>(mappingError);
        }

        return Result.Success(server);
    }

    public async Task<Result<PostmarkServer>> EditServerAsync(ServerEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The server edit parameters cannot be null.");

        var mappedRequest = CreateServerRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<PostmarkServer>(requestError);

        Result<ServerResponseModel> response;
        try
        {
            response = await postmark.PutAsync<ServerRequestModel, ServerResponseModel>(PostmarkTokenScope.Server, "/server", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditServerException(ex);
            return Result.Failure<PostmarkServer>(ex);
        }

        if (response.IsFailure(out var error, out var serverModel))
        {
            LogEditServerError(error.Message, error);
            return Result.Failure<PostmarkServer>(error);
        }

        var mapped = CreatePostmarkServer(serverModel, "server edit response");
        if (mapped.IsFailure(out var mappingError, out var server))
        {
            LogEditServerError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkServer>(mappingError);
        }

        return Result.Success(server);
    }

    public async Task<Result<PostmarkServer>> GetServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(serverId, "The server ID");
        if (validationError is not null)
            return Result.Failure<PostmarkServer>(validationError);

        Result<ServerResponseModel> response;
        try
        {
            response = await postmark.GetAsync<ServerResponseModel>(PostmarkTokenScope.Account, $"/servers/{serverId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetServerException(ex);
            return Result.Failure<PostmarkServer>(ex);
        }

        if (response.IsFailure(out var error, out var serverModel))
        {
            LogGetServerError(error.Message, error);
            return Result.Failure<PostmarkServer>(error);
        }

        var mapped = CreatePostmarkServer(serverModel, "server response");
        if (mapped.IsFailure(out var mappingError, out var server))
        {
            LogGetServerError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkServer>(mappingError);
        }

        return Result.Success(server);
    }

    public async Task<Result<PostmarkServer>> CreateServerAsync(ServerCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The server create parameters cannot be null.");

        var mappedRequest = CreateServerRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<PostmarkServer>(requestError);

        Result<ServerResponseModel> response;
        try
        {
            response = await postmark.PostAsync<ServerRequestModel, ServerResponseModel>(PostmarkTokenScope.Account, "/servers", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateServerException(ex);
            return Result.Failure<PostmarkServer>(ex);
        }

        if (response.IsFailure(out var error, out var serverModel))
        {
            LogCreateServerError(error.Message, error);
            return Result.Failure<PostmarkServer>(error);
        }

        var mapped = CreatePostmarkServer(serverModel, "server create response");
        if (mapped.IsFailure(out var mappingError, out var server))
        {
            LogCreateServerError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkServer>(mappingError);
        }

        return Result.Success(server);
    }

    public async Task<Result<PostmarkServer>> EditServerAsync(long serverId, ServerEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The server edit parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateId(serverId, "The server ID");
        if (validationError is not null)
            return Result.Failure<PostmarkServer>(validationError);

        var mappedRequest = CreateServerRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var requestModel))
            return Result.Failure<PostmarkServer>(requestError);

        Result<ServerResponseModel> response;
        try
        {
            response = await postmark.PutAsync<ServerRequestModel, ServerResponseModel>(PostmarkTokenScope.Account, $"/servers/{serverId.ToString(CultureInfo.InvariantCulture)}", requestModel, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditServerException(ex);
            return Result.Failure<PostmarkServer>(ex);
        }

        if (response.IsFailure(out var error, out var serverModel))
        {
            LogEditServerError(error.Message, error);
            return Result.Failure<PostmarkServer>(error);
        }

        var mapped = CreatePostmarkServer(serverModel, "server edit response");
        if (mapped.IsFailure(out var mappingError, out var server))
        {
            LogEditServerError(mappingError.Message, mappingError);
            return Result.Failure<PostmarkServer>(mappingError);
        }

        return Result.Success(server);
    }

    public async Task<Result<ServerPage>> ListServersAsync(int count = 100, int offset = 0, ServerQuery? query = null, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateServerListRequest(count, offset, query);
        if (validationError is not null)
            return Result.Failure<ServerPage>(validationError);

        Result<ServerListModel> response;
        try
        {
            response = await postmark.GetAsync<ServerListModel>(PostmarkTokenScope.Account, BuildServerListEndpoint(count, offset, query), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListServersException(ex);
            return Result.Failure<ServerPage>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListServersError(error.Message, error);
            return Result.Failure<ServerPage>(error);
        }

        var mapped = CreateServerPage(listModel);
        if (mapped.IsFailure(out var mappingError, out var serverPage))
        {
            LogListServersError(mappingError.Message, mappingError);
            return Result.Failure<ServerPage>(mappingError);
        }

        return Result.Success(serverPage);
    }

    public async Task<Result<ServerDeletion>> DeleteServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateId(serverId, "The server ID");
        if (validationError is not null)
            return Result.Failure<ServerDeletion>(validationError);

        Result<ServerDeletionModel> response;
        try
        {
            response = await postmark.DeleteAsync<ServerDeletionModel>(PostmarkTokenScope.Account, $"/servers/{serverId.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteServerException(ex);
            return Result.Failure<ServerDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteServerError(error.Message, error);
            return Result.Failure<ServerDeletion>(error);
        }

        var mapped = CreateServerDeletion(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteServerError(mappingError.Message, mappingError);
            return Result.Failure<ServerDeletion>(mappingError);
        }

        return Result.Success(deletion);
    }

    private static Result<ServerRequestModel> CreateServerRequest(ServerCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateServerName(parameters.Name, "The server create parameters name", true);
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerColor(parameters.Color, "The server create parameters color");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerDeliveryType(parameters.DeliveryType, "The server create parameters delivery type");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerHooks(parameters, "The server create parameters");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerInboundDomain(parameters.InboundDomain, "The server create parameters inbound domain");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerInboundSpamThreshold(parameters.InboundSpamThreshold, "The server create parameters inbound spam threshold");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerLinkTracking(parameters.TrackLinks, "The server create parameters link tracking");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        return Result.Success(new ServerRequestModel
        {
            Name = parameters.Name,
            Color = parameters.Color.HasValue ? GetServerColorValue(parameters.Color.Value) : null,
            SmtpApiActivated = parameters.SmtpApiActivated,
            RawEmailEnabled = parameters.RawEmailEnabled,
            DeliveryType = parameters.DeliveryType.HasValue ? GetServerDeliveryTypeValue(parameters.DeliveryType.Value) : null,
            InboundHookUrl = parameters.InboundHookUrl,
            BounceHookUrl = parameters.BounceHookUrl,
            OpenHookUrl = parameters.OpenHookUrl,
            DeliveryHookUrl = parameters.DeliveryHookUrl,
            PostFirstOpenOnly = parameters.PostFirstOpenOnly,
            InboundDomain = parameters.InboundDomain,
            InboundSpamThreshold = parameters.InboundSpamThreshold,
            TrackOpens = parameters.TrackOpens,
            TrackLinks = parameters.TrackLinks.HasValue ? GetLinkTrackingValue(parameters.TrackLinks.Value) : null,
            IncludeBounceContentInHook = parameters.IncludeBounceContentInHook,
            ClickHookUrl = parameters.ClickHookUrl,
            EnableSmtpApiErrorHooks = parameters.EnableSmtpApiErrorHooks
        });
    }

    private static Result<ServerRequestModel> CreateServerRequest(ServerEditParameters parameters)
    {
        if (parameters.Name is null && parameters.Color is null && parameters.SmtpApiActivated is null && parameters.RawEmailEnabled is null && parameters.InboundHookUrl is null && parameters.BounceHookUrl is null &&
            parameters.OpenHookUrl is null && parameters.DeliveryHookUrl is null && parameters.PostFirstOpenOnly is null && parameters.InboundDomain is null && parameters.InboundSpamThreshold is null && parameters.TrackOpens is null &&
            parameters.TrackLinks is null && parameters.IncludeBounceContentInHook is null && parameters.ClickHookUrl is null && parameters.EnableSmtpApiErrorHooks is null)
            return Result.Failure<ServerRequestModel>("The server edit parameters must set at least one editable property.");

        var validationError = ValidationExtensions.ValidateServerName(parameters.Name, "The server edit parameters name", false);
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerColor(parameters.Color, "The server edit parameters color");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerHooks(parameters, "The server edit parameters");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerInboundDomain(parameters.InboundDomain, "The server edit parameters inbound domain");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerInboundSpamThreshold(parameters.InboundSpamThreshold, "The server edit parameters inbound spam threshold");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateServerLinkTracking(parameters.TrackLinks, "The server edit parameters link tracking");
        if (validationError is not null)
            return Result.Failure<ServerRequestModel>(validationError);

        return Result.Success(new ServerRequestModel
        {
            Name = parameters.Name,
            Color = parameters.Color.HasValue ? GetServerColorValue(parameters.Color.Value) : null,
            SmtpApiActivated = parameters.SmtpApiActivated,
            RawEmailEnabled = parameters.RawEmailEnabled,
            InboundHookUrl = parameters.InboundHookUrl,
            BounceHookUrl = parameters.BounceHookUrl,
            OpenHookUrl = parameters.OpenHookUrl,
            DeliveryHookUrl = parameters.DeliveryHookUrl,
            PostFirstOpenOnly = parameters.PostFirstOpenOnly,
            InboundDomain = parameters.InboundDomain,
            InboundSpamThreshold = parameters.InboundSpamThreshold,
            TrackOpens = parameters.TrackOpens,
            TrackLinks = parameters.TrackLinks.HasValue ? GetLinkTrackingValue(parameters.TrackLinks.Value) : null,
            IncludeBounceContentInHook = parameters.IncludeBounceContentInHook,
            ClickHookUrl = parameters.ClickHookUrl,
            EnableSmtpApiErrorHooks = parameters.EnableSmtpApiErrorHooks
        });
    }

    private static string BuildServerListEndpoint(int count, int offset, ServerQuery? query)
    {
        var parameters = new List<string>(3)
        {
            $"count={count.ToString(CultureInfo.InvariantCulture)}",
            $"offset={offset.ToString(CultureInfo.InvariantCulture)}"
        };

        if (query?.Name is not null)
            parameters.Add($"name={Uri.EscapeDataString(query.Name)}");

        return $"/servers?{string.Join("&", parameters)}";
    }

    private static Result<ServerPage> CreateServerPage(ServerListModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<ServerPage>("TotalCount was not returned from the Postmark Servers API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<ServerPage>($"TotalCount returned from the Postmark Servers API was invalid. Received {response.TotalCount.Value}.");

        if (response.Servers is null)
            return Result.Failure<ServerPage>("Servers were not returned from the Postmark Servers API.");

        var servers = new List<PostmarkServer>(response.Servers.Count);
        for (var index = 0; index < response.Servers.Count; index++)
        {
            var mapped = CreatePostmarkServer(response.Servers[index], $"server list item {index}");
            if (mapped.IsFailure(out var error, out var server))
                return Result.Failure<ServerPage>(error);

            servers.Add(server);
        }

        return Result.Success(new ServerPage { TotalCount = response.TotalCount.Value, Servers = servers });
    }

    private static Result<PostmarkServer> CreatePostmarkServer(ServerResponseModel? response, string context)
    {
        if (response is null)
            return Result.Failure<PostmarkServer>($"{context} returned from the Postmark Servers API was null.");

        if (response.Id is null)
            return Result.Failure<PostmarkServer>($"ID was not returned from the Postmark Servers API for {context}.");

        if (response.Id.Value <= 0)
            return Result.Failure<PostmarkServer>($"ID returned from the Postmark Servers API for {context} was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<PostmarkServer>($"Name was not returned from the Postmark Servers API for {context}.");

        var tokens = CreateServerTokens(response.ApiTokens, context);
        if (tokens.IsFailure(out var tokenError, out var apiTokens))
            return Result.Failure<PostmarkServer>(tokenError);

        if (string.IsNullOrWhiteSpace(response.Color))
            return Result.Failure<PostmarkServer>($"Color was not returned from the Postmark Servers API for {context}.");

        var color = TryMapServerColor(response.Color);
        if (color.IsFailure(out var colorError, out var mappedColor))
            return Result.Failure<PostmarkServer>($"{context} could not be mapped: {colorError.Message}");

        if (response.SmtpApiActivated is null)
            return Result.Failure<PostmarkServer>($"SmtpApiActivated was not returned from the Postmark Servers API for {context}.");

        if (response.RawEmailEnabled is null)
            return Result.Failure<PostmarkServer>($"RawEmailEnabled was not returned from the Postmark Servers API for {context}.");

        if (string.IsNullOrWhiteSpace(response.DeliveryType))
            return Result.Failure<PostmarkServer>($"DeliveryType was not returned from the Postmark Servers API for {context}.");

        var deliveryType = TryMapServerDeliveryType(response.DeliveryType);
        if (deliveryType.IsFailure(out var deliveryTypeError, out var mappedDeliveryType))
            return Result.Failure<PostmarkServer>($"{context} could not be mapped: {deliveryTypeError.Message}");

        if (response.PostFirstOpenOnly is null)
            return Result.Failure<PostmarkServer>($"PostFirstOpenOnly was not returned from the Postmark Servers API for {context}.");

        if (response.InboundSpamThreshold is null)
            return Result.Failure<PostmarkServer>($"InboundSpamThreshold was not returned from the Postmark Servers API for {context}.");

        if (response.InboundSpamThreshold.Value < 0)
            return Result.Failure<PostmarkServer>($"InboundSpamThreshold returned from the Postmark Servers API for {context} was invalid. Received {response.InboundSpamThreshold.Value}.");

        if (response.TrackOpens is null)
            return Result.Failure<PostmarkServer>($"TrackOpens was not returned from the Postmark Servers API for {context}.");

        if (string.IsNullOrWhiteSpace(response.TrackLinks))
            return Result.Failure<PostmarkServer>($"TrackLinks was not returned from the Postmark Servers API for {context}.");

        var trackLinks = TryMapLinkTracking(response.TrackLinks, context);
        if (trackLinks.IsFailure(out var trackLinksError, out var mappedTrackLinks))
            return Result.Failure<PostmarkServer>(trackLinksError);

        if (response.IncludeBounceContentInHook is null)
            return Result.Failure<PostmarkServer>($"IncludeBounceContentInHook was not returned from the Postmark Servers API for {context}.");

        if (response.EnableSmtpApiErrorHooks is null)
            return Result.Failure<PostmarkServer>($"EnableSmtpApiErrorHooks was not returned from the Postmark Servers API for {context}.");

        return Result.Success(new PostmarkServer(response.Id.Value, response.Name, apiTokens, mappedColor, response.SmtpApiActivated.Value, response.RawEmailEnabled.Value, mappedDeliveryType,
            ValidationExtensions.NormalizeOptionalString(response.ServerLink), ValidationExtensions.NormalizeOptionalString(response.InboundAddress), ValidationExtensions.NormalizeOptionalString(response.InboundHookUrl),
            ValidationExtensions.NormalizeOptionalString(response.BounceHookUrl), ValidationExtensions.NormalizeOptionalString(response.OpenHookUrl), ValidationExtensions.NormalizeOptionalString(response.DeliveryHookUrl),
            response.PostFirstOpenOnly.Value, ValidationExtensions.NormalizeOptionalString(response.InboundDomain), ValidationExtensions.NormalizeOptionalString(response.InboundHash), response.InboundSpamThreshold.Value,
            response.TrackOpens.Value, mappedTrackLinks, response.IncludeBounceContentInHook.Value, ValidationExtensions.NormalizeOptionalString(response.ClickHookUrl), response.EnableSmtpApiErrorHooks.Value));
    }

    private static Result<IReadOnlyList<string>> CreateServerTokens(List<string?>? response, string context)
    {
        if (response is null)
            return Result.Failure<IReadOnlyList<string>>($"ApiTokens were not returned from the Postmark Servers API for {context}.");

        var tokens = new List<string>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var token = response[index];
            if (string.IsNullOrWhiteSpace(token))
                return Result.Failure<IReadOnlyList<string>>($"ApiTokens item {index} was not returned from the Postmark Servers API for {context}.");

            tokens.Add(token);
        }

        return Result.Success<IReadOnlyList<string>>(tokens);
    }

    private static Result<ServerDeletion> CreateServerDeletion(ServerDeletionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<ServerDeletion>("ErrorCode was not returned from the Postmark Servers API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<ServerDeletion>("Message was not returned from the Postmark Servers API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<ServerDeletion>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new ServerDeletion { Message = response.Message });
    }

    private static string GetServerColorValue(ServerColor color)
    {
        return color switch
        {
            ServerColor.Purple => "Purple",
            ServerColor.Blue => "Blue",
            ServerColor.Turquoise => "Turquoise",
            ServerColor.Green => "Green",
            ServerColor.Red => "Red",
            ServerColor.Yellow => "Yellow",
            ServerColor.Grey => "Grey",
            ServerColor.Orange => "Orange",
            _ => throw new NotImplementedException($"Server color enum value of '{nameof(ServerColor)}.{color}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<ServerColor> TryMapServerColor(string color)
    {
        var mapped = color switch
        {
            _ when string.Equals(color, "Purple", StringComparison.OrdinalIgnoreCase) => ServerColor.Purple,
            _ when string.Equals(color, "Blue", StringComparison.OrdinalIgnoreCase) => ServerColor.Blue,
            _ when string.Equals(color, "Turquoise", StringComparison.OrdinalIgnoreCase) => ServerColor.Turquoise,
            _ when string.Equals(color, "Green", StringComparison.OrdinalIgnoreCase) => ServerColor.Green,
            _ when string.Equals(color, "Red", StringComparison.OrdinalIgnoreCase) => ServerColor.Red,
            _ when string.Equals(color, "Yellow", StringComparison.OrdinalIgnoreCase) => ServerColor.Yellow,
            _ when string.Equals(color, "Grey", StringComparison.OrdinalIgnoreCase) => ServerColor.Grey,
            _ when string.Equals(color, "Orange", StringComparison.OrdinalIgnoreCase) => ServerColor.Orange,
            _ => (ServerColor?)null
        };

        if (mapped is null)
            return Result.Failure<ServerColor>($"Color value '{color}' returned from the Postmark Servers API is not supported.");

        return Result.Success(mapped.Value);
    }

    private static string GetServerDeliveryTypeValue(ServerDeliveryType deliveryType)
    {
        return deliveryType switch
        {
            ServerDeliveryType.Live => "Live",
            ServerDeliveryType.Sandbox => "Sandbox",
            _ => throw new NotImplementedException(
                $"Server delivery type enum value of '{nameof(ServerDeliveryType)}.{deliveryType}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<ServerDeliveryType> TryMapServerDeliveryType(string deliveryType)
    {
        var mapped = deliveryType switch
        {
            "Live" => ServerDeliveryType.Live,
            "Sandbox" => ServerDeliveryType.Sandbox,
            _ => (ServerDeliveryType?)null
        };

        if (mapped is null)
            return Result.Failure<ServerDeliveryType>($"DeliveryType value '{deliveryType}' returned from the Postmark Servers API is not supported.");

        return Result.Success(mapped.Value);
    }

    private static string GetLinkTrackingValue(LinkTracking tracking)
    {
        return tracking switch
        {
            LinkTracking.None => "None",
            LinkTracking.HtmlAndText => "HtmlAndText",
            LinkTracking.HtmlOnly => "HtmlOnly",
            LinkTracking.TextOnly => "TextOnly",
            _ => throw new NotImplementedException($"Link tracking enum value of '{nameof(LinkTracking)}.{tracking}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<LinkTracking> TryMapLinkTracking(string value, string context)
    {
        var mapped = value switch
        {
            "None" => LinkTracking.None,
            "HtmlAndText" => LinkTracking.HtmlAndText,
            "HtmlOnly" => LinkTracking.HtmlOnly,
            "TextOnly" => LinkTracking.TextOnly,
            _ => (LinkTracking?)null
        };

        if (mapped is null)
            return Result.Failure<LinkTracking>($"TrackLinks value '{value}' returned from the Postmark API for {context} is not supported.");

        return Result.Success(mapped.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the server.")]
    private partial void LogGetServerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the server. {Message}")]
    private partial void LogGetServerError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the server.")]
    private partial void LogCreateServerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the server. {Message}")]
    private partial void LogCreateServerError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the server.")]
    private partial void LogEditServerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the server. {Message}")]
    private partial void LogEditServerError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list servers.")]
    private partial void LogListServersException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list servers. {Message}")]
    private partial void LogListServersError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the server.")]
    private partial void LogDeleteServerException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the server. {Message}")]
    private partial void LogDeleteServerError(string message, [LogProperties] IError error);
}
