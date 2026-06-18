using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Messages;
using PostKit.Postmark;
using TrackingClientModel = PostKit.Postmark.Messages.MessageTrackingClientResponse;
using TrackingClickModel = PostKit.Postmark.Messages.MessageClickResponse;
using TrackingClickSearchModel = PostKit.Postmark.Messages.MessageClickSearchResponse;
using TrackingEventModel = PostKit.Postmark.Messages.MessageTrackingEventResponse;
using TrackingGeoModel = PostKit.Postmark.Messages.MessageTrackingGeoResponse;
using TrackingOpenModel = PostKit.Postmark.Messages.MessageOpenResponse;
using TrackingOpenSearchModel = PostKit.Postmark.Messages.MessageOpenSearchResponse;
using TrackingOsModel = PostKit.Postmark.Messages.MessageTrackingOperatingSystemResponse;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public Task<Result<MessageOpenPage>> SearchMessageOpensAsync(MessageStream messageStream, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, MessageTrackingQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The message opens query message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<MessageOpenPage>(error));

        return SearchMessageOpensAsync(messageStreamId, count, offset, query, cancellationToken);
    }

    public async Task<Result<MessageOpenPage>> SearchMessageOpensAsync(string messageStream, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, MessageTrackingQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateMessageTrackingSearchRequest(messageStream, count, offset, query, "message opens query", true);
        if (validationError is not null)
            return Result.Failure<MessageOpenPage>(validationError);

        Result<TrackingOpenSearchModel> response;
        try
        {
            var endpoint = BuildMessageTrackingSearchEndpoint("/messages/outbound/opens", messageStream, count, offset, query);
            response = await postmark.GetAsync<TrackingOpenSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessageOpensException(ex);
            return Result.Failure<MessageOpenPage>(ex);
        }

        if (response.IsFailure(out var error, out var openSearchModel))
        {
            LogMessageOpensError(error.Message, error);
            return Result.Failure<MessageOpenPage>(error);
        }

        var mapped = CreateMessageOpenPage(openSearchModel);
        if (mapped.IsFailure(out var mappingError, out var openPage))
        {
            LogMessageOpensError(mappingError.Message, mappingError);
            return Result.Failure<MessageOpenPage>(mappingError);
        }

        return Result.Success(openPage);
    }

    public async Task<Result<MessageOpenPage>> GetMessageOpensAsync(Guid messageId, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateSingleMessageTrackingRequest(messageId, count, offset, "message opens");
        if (validationError is not null)
            return Result.Failure<MessageOpenPage>(validationError);

        Result<TrackingOpenSearchModel> response;
        try
        {
            var endpoint = BuildSingleMessageTrackingEndpoint($"/messages/outbound/opens/{messageId:D}", count, offset);
            response = await postmark.GetAsync<TrackingOpenSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessageOpensException(ex);
            return Result.Failure<MessageOpenPage>(ex);
        }

        if (response.IsFailure(out var error, out var openSearchModel))
        {
            LogMessageOpensError(error.Message, error);
            return Result.Failure<MessageOpenPage>(error);
        }

        var mapped = CreateMessageOpenPage(openSearchModel);
        if (mapped.IsFailure(out var mappingError, out var openPage))
        {
            LogMessageOpensError(mappingError.Message, mappingError);
            return Result.Failure<MessageOpenPage>(mappingError);
        }

        return Result.Success(openPage);
    }

    public Task<Result<MessageClickPage>> SearchMessageClicksAsync(MessageStream messageStream, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, MessageTrackingQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The message clicks query message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<MessageClickPage>(error));

        return SearchMessageClicksAsync(messageStreamId, count, offset, query, cancellationToken);
    }

    public async Task<Result<MessageClickPage>> SearchMessageClicksAsync(string messageStream, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, MessageTrackingQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateMessageTrackingSearchRequest(messageStream, count, offset, query, "message clicks query", true);
        if (validationError is not null)
            return Result.Failure<MessageClickPage>(validationError);

        Result<TrackingClickSearchModel> response;
        try
        {
            var endpoint = BuildMessageTrackingSearchEndpoint("/messages/outbound/clicks", messageStream, count, offset, query);
            response = await postmark.GetAsync<TrackingClickSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessageClicksException(ex);
            return Result.Failure<MessageClickPage>(ex);
        }

        if (response.IsFailure(out var error, out var clickSearchModel))
        {
            LogMessageClicksError(error.Message, error);
            return Result.Failure<MessageClickPage>(error);
        }

        var mapped = CreateMessageClickPage(clickSearchModel);
        if (mapped.IsFailure(out var mappingError, out var clickPage))
        {
            LogMessageClicksError(mappingError.Message, mappingError);
            return Result.Failure<MessageClickPage>(mappingError);
        }

        return Result.Success(clickPage);
    }

    public async Task<Result<MessageClickPage>> GetMessageClicksAsync(Guid messageId, int count = ValidationExtensions.MaxMessageTrackingCount, int offset = 0, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateSingleMessageTrackingRequest(messageId, count, offset, "message clicks");
        if (validationError is not null)
            return Result.Failure<MessageClickPage>(validationError);

        Result<TrackingClickSearchModel> response;
        try
        {
            var endpoint = BuildSingleMessageTrackingEndpoint($"/messages/outbound/clicks/{messageId:D}", count, offset);
            response = await postmark.GetAsync<TrackingClickSearchModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessageClicksException(ex);
            return Result.Failure<MessageClickPage>(ex);
        }

        if (response.IsFailure(out var error, out var clickSearchModel))
        {
            LogMessageClicksError(error.Message, error);
            return Result.Failure<MessageClickPage>(error);
        }

        var mapped = CreateMessageClickPage(clickSearchModel);
        if (mapped.IsFailure(out var mappingError, out var clickPage))
        {
            LogMessageClicksError(mappingError.Message, mappingError);
            return Result.Failure<MessageClickPage>(mappingError);
        }

        return Result.Success(clickPage);
    }

    private static string BuildMessageTrackingSearchEndpoint(string endpoint, string messageStream, int count, int offset, MessageTrackingQuery? query)
    {
        var parameters = new List<string>(15)
        {
            $"count={count.ToString(CultureInfo.InvariantCulture)}",
            $"offset={offset.ToString(CultureInfo.InvariantCulture)}"
        };

        if (query?.Recipient is not null)
            parameters.Add($"recipient={Uri.EscapeDataString(query.Recipient.Address)}");

        if (query?.Tag is not null)
            parameters.Add($"tag={Uri.EscapeDataString(query.Tag)}");

        parameters.Add($"messagestream={Uri.EscapeDataString(messageStream)}");

        AddMessageTrackingFilter(parameters, "client_name", query?.ClientName);
        AddMessageTrackingFilter(parameters, "client_company", query?.ClientCompany);
        AddMessageTrackingFilter(parameters, "client_family", query?.ClientFamily);
        AddMessageTrackingFilter(parameters, "os_name", query?.OsName);
        AddMessageTrackingFilter(parameters, "os_family", query?.OsFamily);
        AddMessageTrackingFilter(parameters, "os_company", query?.OsCompany);
        AddMessageTrackingFilter(parameters, "platform", query?.Platform);
        AddMessageTrackingFilter(parameters, "country", query?.Country);
        AddMessageTrackingFilter(parameters, "region", query?.Region);
        AddMessageTrackingFilter(parameters, "city", query?.City);

        return $"{endpoint}?{string.Join("&", parameters)}";
    }

    private static string BuildSingleMessageTrackingEndpoint(string endpoint, int count, int offset)
    {
        return $"{endpoint}?count={count.ToString(CultureInfo.InvariantCulture)}&offset={offset.ToString(CultureInfo.InvariantCulture)}";
    }

    private static void AddMessageTrackingFilter(List<string> parameters, string name, string? value)
    {
        if (value is not null)
            parameters.Add($"{name}={Uri.EscapeDataString(value)}");
    }

    private static Result<MessageOpenPage> CreateMessageOpenPage(TrackingOpenSearchModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<MessageOpenPage>("TotalCount was not returned from the Postmark Messages API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<MessageOpenPage>($"TotalCount returned from the Postmark Messages API was invalid. Received {response.TotalCount.Value}.");

        if (response.Opens is null)
            return Result.Failure<MessageOpenPage>("Opens were not returned from the Postmark Messages API.");

        var opens = new List<MessageOpen>(response.Opens.Count);
        for (var index = 0; index < response.Opens.Count; index++)
        {
            var mapped = CreateMessageOpen(response.Opens[index], index);
            if (mapped.IsFailure(out var error, out var open))
                return Result.Failure<MessageOpenPage>(error);

            opens.Add(open);
        }

        return Result.Success(new MessageOpenPage { TotalCount = response.TotalCount.Value, Opens = opens });
    }

    private static Result<MessageClickPage> CreateMessageClickPage(TrackingClickSearchModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<MessageClickPage>("TotalCount was not returned from the Postmark Messages API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<MessageClickPage>($"TotalCount returned from the Postmark Messages API was invalid. Received {response.TotalCount.Value}.");

        if (response.Clicks is null)
            return Result.Failure<MessageClickPage>("Clicks were not returned from the Postmark Messages API.");

        var clicks = new List<MessageClick>(response.Clicks.Count);
        for (var index = 0; index < response.Clicks.Count; index++)
        {
            var mapped = CreateMessageClick(response.Clicks[index], index);
            if (mapped.IsFailure(out var error, out var click))
                return Result.Failure<MessageClickPage>(error);

            clicks.Add(click);
        }

        return Result.Success(new MessageClickPage { TotalCount = response.TotalCount.Value, Clicks = clicks });
    }

    private static Result<MessageOpen> CreateMessageOpen(TrackingOpenModel? response, int index)
    {
        if (response is null)
            return Result.Failure<MessageOpen>($"Open item {index} returned from the Postmark Messages API was null.");

        var common = CreateMessageTrackingEventCore(response, $"Open item {index}");
        if (common.IsFailure(out var error, out var core))
            return Result.Failure<MessageOpen>(error);

        return Result.Success(new MessageOpen(core.RecordType, core.Client, core.Os, core.Platform, core.UserAgent, core.Geo, core.MessageId, core.MessageStream, core.ReceivedAt, core.Tag, core.Recipient));
    }

    private static Result<MessageClick> CreateMessageClick(TrackingClickModel? response, int index)
    {
        if (response is null)
            return Result.Failure<MessageClick>($"Click item {index} returned from the Postmark Messages API was null.");

        var common = CreateMessageTrackingEventCore(response, $"Click item {index}");
        if (common.IsFailure(out var error, out var core))
            return Result.Failure<MessageClick>(error);

        return Result.Success(new MessageClick(core.RecordType, response.ClickLocation, core.Client, core.Os, response.OriginalLink, core.Platform, core.UserAgent, core.Geo, core.MessageId, core.MessageStream, core.ReceivedAt, core.Tag,
            core.Recipient));
    }

    private static Result<MessageTrackingEventCore> CreateMessageTrackingEventCore(TrackingEventModel response, string context)
    {
        if (string.IsNullOrWhiteSpace(response.UserAgent))
            return Result.Failure<MessageTrackingEventCore>($"UserAgent was not returned from the Postmark Messages API for {context}.");

        if (string.IsNullOrWhiteSpace(response.MessageId))
            return Result.Failure<MessageTrackingEventCore>($"MessageID was not returned from the Postmark Messages API for {context}.");

        if (!Guid.TryParse(response.MessageId, out var messageId))
            return Result.Failure<MessageTrackingEventCore>($"MessageID returned from the Postmark Messages API for {context} was not a valid GUID.");

        if (string.IsNullOrWhiteSpace(response.MessageStream))
            return Result.Failure<MessageTrackingEventCore>($"MessageStream was not returned from the Postmark Messages API for {context}.");

        if (response.ReceivedAt is null)
            return Result.Failure<MessageTrackingEventCore>($"ReceivedAt was not returned from the Postmark Messages API for {context}.");

        return Result.Success(new MessageTrackingEventCore(NormalizeOptionalMessageTrackingString(response.RecordType), CreateMessageTrackingClient(response.Client), CreateMessageTrackingOperatingSystem(response.Os),
            NormalizeOptionalMessageTrackingString(response.Platform), response.UserAgent, CreateMessageTrackingGeo(response.Geo), messageId, response.MessageStream, response.ReceivedAt.Value,
            NormalizeOptionalMessageTrackingString(response.Tag), NormalizeOptionalMessageTrackingString(response.Recipient)));
    }

    private static MessageTrackingClient? CreateMessageTrackingClient(TrackingClientModel? response)
    {
        return response is null
            ? null
            : new MessageTrackingClient
            {
                Name = response.Name,
                Company = response.Company,
                Family = response.Family
            };
    }

    private static MessageTrackingOperatingSystem? CreateMessageTrackingOperatingSystem(TrackingOsModel? response)
    {
        return response is null
            ? null
            : new MessageTrackingOperatingSystem
            {
                Name = response.Name,
                Company = response.Company,
                Family = response.Family
            };
    }

    private static MessageTrackingGeo? CreateMessageTrackingGeo(TrackingGeoModel? response)
    {
        return response is null ? null : new MessageTrackingGeo(response.CountryIsoCode, response.Country, response.RegionIsoCode, response.Region, response.City, response.Zip, response.Coords, response.Ip);
    }

    private static string? NormalizeOptionalMessageTrackingString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve message opens.")]
    private partial void LogMessageOpensException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve message opens. {Message}")]
    private partial void LogMessageOpensError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve message clicks.")]
    private partial void LogMessageClicksException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve message clicks. {Message}")]
    private partial void LogMessageClicksError(string message, [LogProperties] IError error);

    private readonly record struct MessageTrackingEventCore(
        string? RecordType,
        MessageTrackingClient? Client,
        MessageTrackingOperatingSystem? Os,
        string? Platform,
        string UserAgent,
        MessageTrackingGeo? Geo,
        Guid MessageId,
        string MessageStream,
        DateTimeOffset ReceivedAt,
        string? Tag,
        string? Recipient);
}
