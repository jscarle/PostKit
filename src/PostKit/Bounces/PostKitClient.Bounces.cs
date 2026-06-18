using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Bounces;
using PostKit.Common;
using PostKit.Postmark;
using ActivateBounceModel = PostKit.Postmark.Bounces.ActivateBounceResponse;
using BounceModel = PostKit.Postmark.Bounces.BounceResponse;
using GetBounceDumpModel = PostKit.Postmark.Bounces.GetBounceDumpResponse;
using GetBouncesModel = PostKit.Postmark.Bounces.GetBouncesResponse;
using GetDeliveryStatsModel = PostKit.Postmark.Bounces.GetDeliveryStatsResponse;
using BounceCountElementModel = PostKit.Postmark.Bounces.BounceCountElement;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    private const int BounceActivationConfirmationAttempts = 30;
    private static readonly TimeSpan BounceActivationConfirmationDelay = TimeSpan.FromSeconds(2);

    public Task<Result<BouncePage>> GetBouncesAsync(MessageStream messageStream, int count = ValidationExtensions.MaxBounceCount, int offset = 0, BounceQuery? query = null, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The bounce query message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<BouncePage>(error));

        return GetBouncesAsync(messageStreamId, count, offset, query, cancellationToken);
    }

    public async Task<Result<BouncePage>> GetBouncesAsync(string messageStream, int count = ValidationExtensions.MaxBounceCount, int offset = 0, BounceQuery? query = null, CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateBounceQuery(messageStream, count, offset, query);
        if (validationError is not null)
            return Result.Failure<BouncePage>(validationError);

        Result<GetBouncesModel> response;
        try
        {
            var endpoint = BuildBounceSearchEndpoint(messageStream, count, offset, query);
            response = await postmark.GetAsync<GetBouncesModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBouncesException(ex);
            return Result.Failure<BouncePage>(ex);
        }

        if (response.IsFailure(out var error, out var bounceSearchModel))
        {
            LogBouncesError(error.Message, error);
            return Result.Failure<BouncePage>(error);
        }

        var mappedResponse = CreateGetBouncesResponse(bounceSearchModel);
        if (mappedResponse.IsFailure(out var mappingError, out var getBouncesResponse))
        {
            LogBouncesError(mappingError.Message, mappingError);
            return Result.Failure<BouncePage>(mappingError);
        }

        return Result.Success(getBouncesResponse);
    }

    public async Task<Result<BounceDetails>> GetBounceAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Failure<BounceDetails>($"The bounce ID must be greater than zero. Received {id}.");

        Result<BounceModel> response;
        try
        {
            response = await postmark.GetAsync<BounceModel>(PostmarkTokenScope.Server, $"/bounces/{id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBounceException(ex);
            return Result.Failure<BounceDetails>(ex);
        }

        if (response.IsFailure(out var error, out var bounceModel))
        {
            LogBounceError(error.Message, error);
            return Result.Failure<BounceDetails>(error);
        }

        var mappedResponse = CreateGetBounceResponse(bounceModel);
        if (mappedResponse.IsFailure(out var mappingError, out var getBounceResponse))
        {
            LogBounceError(mappingError.Message, mappingError);
            return Result.Failure<BounceDetails>(mappingError);
        }

        return Result.Success(getBounceResponse);
    }

    public async Task<Result<DeliveryStats>> GetDeliveryStatsAsync(CancellationToken cancellationToken = default)
    {
        Result<GetDeliveryStatsModel> response;
        try
        {
            response = await postmark.GetAsync<GetDeliveryStatsModel>(PostmarkTokenScope.Server, "/deliverystats", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeliveryStatsException(ex);
            return Result.Failure<DeliveryStats>(ex);
        }

        if (response.IsFailure(out var error, out var deliveryStatsModel))
        {
            LogDeliveryStatsError(error.Message, error);
            return Result.Failure<DeliveryStats>(error);
        }

        var mappedResponse = CreateGetDeliveryStatsResponse(deliveryStatsModel);
        if (mappedResponse.IsFailure(out var mappingError, out var getDeliveryStatsResponse))
        {
            LogDeliveryStatsError(mappingError.Message, mappingError);
            return Result.Failure<DeliveryStats>(mappingError);
        }

        return Result.Success(getDeliveryStatsResponse);
    }

    public async Task<Result<BounceDump>> GetBounceDumpAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Failure<BounceDump>($"The bounce ID must be greater than zero. Received {id}.");

        Result<GetBounceDumpModel> response;
        try
        {
            response = await postmark.GetAsync<GetBounceDumpModel>(PostmarkTokenScope.Server, $"/bounces/{id}/dump", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogBounceDumpException(ex);
            return Result.Failure<BounceDump>(ex);
        }

        if (response.IsFailure(out var error, out var bounceDumpModel))
        {
            LogBounceDumpError(error.Message, error);
            return Result.Failure<BounceDump>(error);
        }

        if (bounceDumpModel.Body is null)
            return Result.Failure<BounceDump>("Body was not returned from the Postmark Bounces API.");

        var bounceDump = new BounceDump(bounceDumpModel.Body);
        return Result.Success(bounceDump);
    }

    public async Task<Result<BounceActivation>> ActivateBounceAsync(long id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return Result.Failure<BounceActivation>($"The bounce ID must be greater than zero. Received {id}.");

        Result<ActivateBounceModel> response;
        try
        {
            response = await postmark.PutAsync<ActivateBounceModel>(PostmarkTokenScope.Server, $"/bounces/{id}/activate", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogActivateBounceException(ex);
            return Result.Failure<BounceActivation>(ex);
        }

        if (response.IsFailure(out var error, out var bounceActivationModel))
        {
            LogActivateBounceError(error.Message, error);
            return Result.Failure<BounceActivation>(error);
        }

        if (string.IsNullOrWhiteSpace(bounceActivationModel.Message))
            return Result.Failure<BounceActivation>("Message was not returned from the Postmark Bounces API.");

        if (bounceActivationModel.Bounce is null)
            return Result.Failure<BounceActivation>("Bounce was not returned from the Postmark Bounces API.");

        var mappedBounce = CreateBounce(bounceActivationModel.Bounce);
        if (mappedBounce.IsFailure(out var mappingError, out var bounce))
        {
            LogActivateBounceError(mappingError.Message, mappingError);
            return Result.Failure<BounceActivation>(mappingError);
        }

        var confirmedBounce = await ConfirmActivatedBounceAsync(id, bounce, cancellationToken);
        if (confirmedBounce.IsFailure(out var confirmationError, out var activatedBounce))
            return Result.Failure<BounceActivation>(confirmationError);

        var bounceActivation = new BounceActivation(bounceActivationModel.Message, activatedBounce);
        return Result.Success(bounceActivation);
    }

    private async Task<Result<Bounce>> ConfirmActivatedBounceAsync(long id, Bounce fallbackBounce, CancellationToken cancellationToken)
    {
        var currentBounce = fallbackBounce;
        IError? lastError = null;
        Exception? lastException = null;

        if (!currentBounce.Inactive)
            return Result.Success(currentBounce);

        for (var attempt = 0; attempt < BounceActivationConfirmationAttempts; attempt++)
        {
            Result<BounceModel> response = default;
            var hasResponse = false;
            try
            {
                response = await postmark.GetAsync<BounceModel>(PostmarkTokenScope.Server, $"/bounces/{id}", cancellationToken);
                hasResponse = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                LogActivateBounceConfirmationCanceled(id);
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
                lastError = null;
            }

            if (hasResponse)
            {
                if (response.IsFailure(out var error, out var bounceModel))
                {
                    lastError = error;
                    lastException = null;
                }
                else
                {
                    var mappedBounce = CreateBounce(bounceModel);
                    if (mappedBounce.IsFailure(out var mappingError, out var confirmedBounce))
                    {
                        lastError = mappingError;
                        lastException = null;
                    }
                    else
                    {
                        currentBounce = confirmedBounce;
                        lastError = null;
                        lastException = null;

                        if (!confirmedBounce.Inactive)
                            return Result.Success(confirmedBounce);
                    }
                }
            }

            if (attempt < BounceActivationConfirmationAttempts - 1)
                try
                {
                    await Task.Delay(BounceActivationConfirmationDelay, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    LogActivateBounceConfirmationCanceled(id);
                    throw;
                }
        }

        if (lastError is not null)
        {
            LogActivateBounceConfirmationFailure(id, lastError.Message, lastError);
            return Result.Failure<Bounce>(lastError);
        }

        if (lastException is not null)
        {
            LogActivateBounceConfirmationException(id, lastException);
            return Result.Failure<Bounce>(lastException);
        }

        if (currentBounce.Inactive)
        {
            LogActivateBounceConfirmationTimedOut(id);
            return Result.Failure<Bounce>($"Bounce {id} could not be confirmed as active after the activation request.");
        }

        return Result.Success(currentBounce);
    }

    private static string BuildBounceSearchEndpoint(string messageStream, int count, int offset, BounceQuery? query)
    {
        var parameters = new List<string>(10) { $"count={count.ToString(CultureInfo.InvariantCulture)}", $"offset={offset.ToString(CultureInfo.InvariantCulture)}" };

        if (query?.Type.HasValue == true)
            parameters.Add($"type={Uri.EscapeDataString(GetBounceTypeValue(query.Type.Value))}");

        if (query?.Inactive.HasValue == true)
            parameters.Add($"inactive={query.Inactive.Value.ToString().ToLowerInvariant()}");

        if (query?.EmailFilter is not null)
            parameters.Add($"emailFilter={Uri.EscapeDataString(query.EmailFilter.Address)}");

        if (query?.MessageId.HasValue == true)
            parameters.Add($"messageID={query.MessageId.Value:D}");

        if (query?.Tag is not null)
            parameters.Add($"tag={Uri.EscapeDataString(query.Tag)}");

        if (query?.ToDate.HasValue == true)
            parameters.Add($"todate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.ToDate.Value))}");

        if (query?.FromDate.HasValue == true)
            parameters.Add($"fromdate={Uri.EscapeDataString(FormatPostmarkDateQueryValue(query.FromDate.Value))}");

        parameters.Add($"messagestream={Uri.EscapeDataString(messageStream)}");

        return $"/bounces?{string.Join("&", parameters)}";
    }

    private static string FormatPostmarkDateQueryValue(DateTimeOffset value)
    {
        var useDateOnlyFormat = value.TimeOfDay == TimeSpan.Zero;
        var normalizedValue = value.ToEasternStandardDateTimeOffset();

        return normalizedValue.ToString(useDateOnlyFormat ? "yyyy-MM-dd" : "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    }

    private static Result<BouncePage> CreateGetBouncesResponse(GetBouncesModel response)
    {
        if (response.TotalCount is null)
            return Result.Failure<BouncePage>("TotalCount was not returned from the Postmark Bounces API.");

        if (response.TotalCount.Value < 0)
            return Result.Failure<BouncePage>($"TotalCount returned from the Postmark Bounces API was invalid. Received {response.TotalCount.Value}.");

        if (response.Bounces is null)
            return Result.Failure<BouncePage>("Bounces were not returned from the Postmark Bounces API.");

        var bounces = new List<Bounce>(response.Bounces.Count);
        for (var index = 0; index < response.Bounces.Count; index++)
        {
            var mappedBounce = CreateBounce(response.Bounces[index]);
            if (mappedBounce.IsFailure(out var error, out var bounce))
                return Result.Failure<BouncePage>($"Bounce item {index} could not be mapped: {error.Message}");

            bounces.Add(bounce);
        }

        var bouncePage = new BouncePage(response.TotalCount.Value, bounces);
        return Result.Success(bouncePage);
    }

    private static Result<Bounce> CreateBounce(BounceModel response)
    {
        var mappedCore = CreateBounceCore(response);
        if (mappedCore.IsFailure(out var error, out var bounceCore))
            return Result.Failure<Bounce>(error);

        var bounce = new Bounce(bounceCore.RecordType, bounceCore.Id, bounceCore.Type, bounceCore.Name, bounceCore.Tag, bounceCore.MessageId, bounceCore.ServerId, bounceCore.MessageStream, bounceCore.Description, bounceCore.Details,
            bounceCore.Email, bounceCore.From, bounceCore.BouncedAt, bounceCore.DumpAvailable, bounceCore.Inactive, bounceCore.CanActivate, bounceCore.Subject);
        return Result.Success(bounce);
    }

    private static Result<BounceDetails> CreateGetBounceResponse(BounceModel response)
    {
        var mappedCore = CreateBounceCore(response);
        if (mappedCore.IsFailure(out var error, out var bounceCore))
            return Result.Failure<BounceDetails>(error);

        if (response.Content is null)
            return Result.Failure<BounceDetails>("Content was not returned from the Postmark Bounces API.");

        var bounceDetails = new BounceDetails(bounceCore.RecordType, bounceCore.Id, bounceCore.Type, bounceCore.Name, bounceCore.Tag, bounceCore.MessageId, bounceCore.ServerId, bounceCore.MessageStream, bounceCore.Description,
            bounceCore.Details, bounceCore.Email, bounceCore.From, bounceCore.BouncedAt, bounceCore.DumpAvailable, bounceCore.Inactive, bounceCore.CanActivate, bounceCore.Subject, response.Content);
        return Result.Success(bounceDetails);
    }

    private static Result<DeliveryStats> CreateGetDeliveryStatsResponse(GetDeliveryStatsModel response)
    {
        if (response.InactiveMails is null)
            return Result.Failure<DeliveryStats>("InactiveMails was not returned from the Postmark Bounces API.");

        if (response.InactiveMails.Value < 0)
            return Result.Failure<DeliveryStats>($"InactiveMails returned from the Postmark Bounces API was invalid. Received {response.InactiveMails.Value}.");

        if (response.Bounces is null)
            return Result.Failure<DeliveryStats>("Bounces were not returned from the Postmark Bounces API.");

        var bounces = new List<BounceSummary>(response.Bounces.Count);
        for (var index = 0; index < response.Bounces.Count; index++)
        {
            var mappedSummary = CreateBounceSummary(response.Bounces[index]);
            if (mappedSummary.IsFailure(out var error, out var bounceSummary))
                return Result.Failure<DeliveryStats>($"Delivery stats bounce item {index} could not be mapped: {error.Message}");

            bounces.Add(bounceSummary);
        }

        var deliveryStats = new DeliveryStats(response.InactiveMails.Value, bounces);
        return Result.Success(deliveryStats);
    }

    private static Result<BounceSummary> CreateBounceSummary(BounceCountElementModel response)
    {
        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<BounceSummary>("Name was not returned from the Postmark Bounces API.");

        if (response.Count is null)
            return Result.Failure<BounceSummary>("Count was not returned from the Postmark Bounces API.");

        if (response.Count.Value < 0)
            return Result.Failure<BounceSummary>($"Count returned from the Postmark Bounces API was invalid. Received {response.Count.Value}.");

        BounceType? type = null;
        if (!string.IsNullOrWhiteSpace(response.Type))
        {
            var mappedType = TryMapBounceType(response.Type);
            if (mappedType.IsFailure(out var error, out var bounceType))
                return Result.Failure<BounceSummary>(error);

            type = bounceType;
        }

        var bounceSummary = new BounceSummary(type, response.Name, response.Count.Value);
        return Result.Success(bounceSummary);
    }

    private static Result<BounceCore> CreateBounceCore(BounceModel response)
    {
        var recordType = string.IsNullOrWhiteSpace(response.RecordType) ? "Bounce" : response.RecordType;

        if (response.Id is null)
            return Result.Failure<BounceCore>("ID was not returned from the Postmark Bounces API.");

        if (response.Id.Value <= 0)
            return Result.Failure<BounceCore>($"ID returned from the Postmark Bounces API was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Type))
            return Result.Failure<BounceCore>("Type was not returned from the Postmark Bounces API.");

        var mappedType = TryMapBounceType(response.Type);
        if (mappedType.IsFailure(out var typeError, out var type))
            return Result.Failure<BounceCore>(typeError);

        if (string.IsNullOrWhiteSpace(response.Name))
            return Result.Failure<BounceCore>("Name was not returned from the Postmark Bounces API.");

        if (response.Tag is null)
            return Result.Failure<BounceCore>("Tag was not returned from the Postmark Bounces API.");

        if (response.MessageId is null)
            return Result.Failure<BounceCore>("MessageID was not returned from the Postmark Bounces API.");

        if (!Guid.TryParse(response.MessageId, out var messageId))
            return Result.Failure<BounceCore>("MessageID returned from the Postmark Bounces API was not a valid GUID.");

        if (response.ServerId is null)
            return Result.Failure<BounceCore>("ServerID was not returned from the Postmark Bounces API.");

        if (response.ServerId.Value <= 0)
            return Result.Failure<BounceCore>($"ServerID returned from the Postmark Bounces API was invalid. Received {response.ServerId.Value}.");

        if (string.IsNullOrWhiteSpace(response.MessageStream))
            return Result.Failure<BounceCore>("MessageStream was not returned from the Postmark Bounces API.");

        if (string.IsNullOrWhiteSpace(response.Description))
            return Result.Failure<BounceCore>("Description was not returned from the Postmark Bounces API.");

        if (string.IsNullOrWhiteSpace(response.Details))
            return Result.Failure<BounceCore>("Details was not returned from the Postmark Bounces API.");

        if (string.IsNullOrWhiteSpace(response.Email))
            return Result.Failure<BounceCore>("Email was not returned from the Postmark Bounces API.");

        if (response.BouncedAt is null)
            return Result.Failure<BounceCore>("BouncedAt was not returned from the Postmark Bounces API.");

        if (response.DumpAvailable is null)
            return Result.Failure<BounceCore>("DumpAvailable was not returned from the Postmark Bounces API.");

        if (response.Inactive is null)
            return Result.Failure<BounceCore>("Inactive was not returned from the Postmark Bounces API.");

        if (response.CanActivate is null)
            return Result.Failure<BounceCore>("CanActivate was not returned from the Postmark Bounces API.");

        if (response.Subject is null)
            return Result.Failure<BounceCore>("Subject was not returned from the Postmark Bounces API.");

        var bounceCore = new BounceCore(recordType, response.Id.Value, type, response.Name, response.Tag, messageId, response.ServerId.Value, response.MessageStream, response.Description, response.Details, response.Email,
            string.IsNullOrWhiteSpace(response.From) ? null : response.From, response.BouncedAt.Value, response.DumpAvailable.Value, response.Inactive.Value, response.CanActivate.Value, response.Subject);
        return Result.Success(bounceCore);
    }

    private static string GetBounceTypeValue(BounceType type)
    {
        return type switch
        {
            BounceType.HardBounce => "HardBounce",
            BounceType.Transient => "Transient",
            BounceType.Unsubscribe => "Unsubscribe",
            BounceType.Subscribe => "Subscribe",
            BounceType.AutoResponder => "AutoResponder",
            BounceType.AddressChange => "AddressChange",
            BounceType.DnsError => "DnsError",
            BounceType.SpamNotification => "SpamNotification",
            BounceType.OpenRelayTest => "OpenRelayTest",
            BounceType.Unknown => "Unknown",
            BounceType.SoftBounce => "SoftBounce",
            BounceType.VirusNotification => "VirusNotification",
            BounceType.BadEmailAddress => "BadEmailAddress",
            BounceType.SpamComplaint => "SpamComplaint",
            BounceType.ManuallyDeactivated => "ManuallyDeactivated",
            BounceType.Unconfirmed => "Unconfirmed",
            BounceType.Blocked => "Blocked",
            BounceType.SmtpApiError => "SMTPApiError",
            BounceType.InboundError => "InboundError",
            BounceType.DmarcPolicy => "DMARCPolicy",
            BounceType.TemplateRenderingFailed => "TemplateRenderingFailed",
            BounceType.ChallengeVerification => "ChallengeVerification",
            _ => throw new NotImplementedException($"Bounce type enum value of '{nameof(BounceType)}.{type}' has not been implemented. Please open an issue in the PostKit repository (https://github.com/jscarle/PostKit/issues).")
        };
    }

    private static Result<BounceType> TryMapBounceType(string type)
    {
        var mappedType = type switch
        {
            "HardBounce" => BounceType.HardBounce,
            "Transient" => BounceType.Transient,
            "Unsubscribe" => BounceType.Unsubscribe,
            "Subscribe" => BounceType.Subscribe,
            "AutoResponder" => BounceType.AutoResponder,
            "AddressChange" => BounceType.AddressChange,
            "DnsError" => BounceType.DnsError,
            "SpamNotification" => BounceType.SpamNotification,
            "OpenRelayTest" => BounceType.OpenRelayTest,
            "Unknown" => BounceType.Unknown,
            "SoftBounce" => BounceType.SoftBounce,
            "VirusNotification" => BounceType.VirusNotification,
            "BadEmailAddress" => BounceType.BadEmailAddress,
            "SpamComplaint" => BounceType.SpamComplaint,
            "ManuallyDeactivated" => BounceType.ManuallyDeactivated,
            "Unconfirmed" => BounceType.Unconfirmed,
            "Blocked" => BounceType.Blocked,
            "SMTPApiError" => BounceType.SmtpApiError,
            "InboundError" => BounceType.InboundError,
            "DMARCPolicy" => BounceType.DmarcPolicy,
            "TemplateRenderingFailed" => BounceType.TemplateRenderingFailed,
            "ChallengeVerification" => BounceType.ChallengeVerification,
            _ => (BounceType?)null
        };

        if (mappedType is null)
            return Result.Failure<BounceType>($"Bounce type value '{type}' returned from the Postmark Bounces API is not supported.");

        return Result.Success(mappedType.Value);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve bounces.")]
    private partial void LogBouncesException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve bounces. {Message}")]
    private partial void LogBouncesError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve the bounce.")]
    private partial void LogBounceException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve the bounce. {Message}")]
    private partial void LogBounceError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve delivery stats.")]
    private partial void LogDeliveryStatsException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve delivery stats. {Message}")]
    private partial void LogDeliveryStatsError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to retrieve the bounce dump.")]
    private partial void LogBounceDumpException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to retrieve the bounce dump. {Message}")]
    private partial void LogBounceDumpError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to activate the bounce.")]
    private partial void LogActivateBounceException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to activate the bounce. {Message}")]
    private partial void LogActivateBounceError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Warning, "Failed to confirm bounce {BounceId} state after activation. {Message}")]
    private partial void LogActivateBounceConfirmationFailure(long bounceId, string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Warning, "An exception occurred while confirming bounce {BounceId} state after activation.")]
    private partial void LogActivateBounceConfirmationException(long bounceId, Exception ex);

    [LoggerMessage(LogLevel.Warning, "Cancellation was requested while confirming bounce {BounceId} state after activation.")]
    private partial void LogActivateBounceConfirmationCanceled(long bounceId);

    [LoggerMessage(LogLevel.Warning, "Timed out while confirming bounce {BounceId} state after activation.")]
    private partial void LogActivateBounceConfirmationTimedOut(long bounceId);

    private readonly record struct BounceCore(
        string RecordType,
        long Id,
        BounceType Type,
        string Name,
        string Tag,
        Guid MessageId,
        long ServerId,
        string MessageStream,
        string Description,
        string Details,
        string Email,
        string? From,
        DateTimeOffset BouncedAt,
        bool DumpAvailable,
        bool Inactive,
        bool CanActivate,
        string Subject);
}