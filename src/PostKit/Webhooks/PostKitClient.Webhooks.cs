using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Webhooks;
using WebhookBasicTriggerModel = PostKit.Postmark.Webhooks.WebhookBasicTriggerModel;
using WebhookContentTriggerModel = PostKit.Postmark.Webhooks.WebhookContentTriggerModel;
using WebhookDeletionModel = PostKit.Postmark.Webhooks.WebhookDeletionResponse;
using WebhookHeaderModel = PostKit.Postmark.Webhooks.WebhookHeaderModel;
using WebhookHttpAuthModel = PostKit.Postmark.Webhooks.WebhookHttpAuthModel;
using WebhookListModel = PostKit.Postmark.Webhooks.WebhookListResponse;
using WebhookOpenTriggerModel = PostKit.Postmark.Webhooks.WebhookOpenTriggerModel;
using WebhookRequestModel = PostKit.Postmark.Webhooks.WebhookRequest;
using WebhookResponseModel = PostKit.Postmark.Webhooks.WebhookResponse;
using WebhookTriggersModel = PostKit.Postmark.Webhooks.WebhookTriggersModel;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public Task<Result<WebhookList>> ListWebhooksAsync(CancellationToken cancellationToken = default)
    {
        return ListWebhooksCoreAsync(null, cancellationToken);
    }

    public Task<Result<WebhookList>> ListWebhooksAsync(MessageStream messageStream, CancellationToken cancellationToken = default)
    {
        var mappedMessageStream = GetMessageStreamId(messageStream, "The webhook list message stream");
        if (mappedMessageStream.IsFailure(out var error, out var messageStreamId))
            return Task.FromResult(Result.Failure<WebhookList>(error));

        return ListWebhooksAsync(messageStreamId, cancellationToken);
    }

    public Task<Result<WebhookList>> ListWebhooksAsync(string messageStream, CancellationToken cancellationToken = default)
    {
        if (messageStream is null)
            throw new ArgumentNullException(nameof(messageStream), "The message stream ID cannot be null.");

        var validationError = ValidationExtensions.ValidateWebhookMessageStream(messageStream, "The webhook list message stream");
        if (validationError is not null)
            return Task.FromResult(Result.Failure<WebhookList>(validationError));

        return ListWebhooksCoreAsync(messageStream, cancellationToken);
    }

    public async Task<Result<Webhook>> GetWebhookAsync(long id, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateWebhookId(id);
        if (validationError is not null)
            return Result.Failure<Webhook>(validationError);

        Result<WebhookResponseModel> response;
        try
        {
            response = await postmark.GetAsync<WebhookResponseModel>(PostmarkTokenScope.Server, $"/webhooks/{id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogGetWebhookException(ex);
            return Result.Failure<Webhook>(ex);
        }

        if (response.IsFailure(out var error, out var webhookModel))
        {
            LogGetWebhookError(error.Message, error);
            return Result.Failure<Webhook>(error);
        }

        var mapped = CreateWebhook(webhookModel, "webhook response");
        if (mapped.IsFailure(out var mappingError, out var webhook))
        {
            LogGetWebhookError(mappingError.Message, mappingError);
            return Result.Failure<Webhook>(mappingError);
        }

        return Result.Success(webhook);
    }

    public async Task<Result<Webhook>> CreateWebhookAsync(WebhookCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The webhook create parameters cannot be null.");

        var mappedRequest = CreateWebhookRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var webhookRequest))
            return Result.Failure<Webhook>(requestError);

        Result<WebhookResponseModel> response;
        try
        {
            response = await postmark.PostAsync<WebhookRequestModel, WebhookResponseModel>(PostmarkTokenScope.Server, "/webhooks", webhookRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogCreateWebhookException(ex);
            return Result.Failure<Webhook>(ex);
        }

        if (response.IsFailure(out var error, out var webhookModel))
        {
            LogCreateWebhookError(error.Message, error);
            return Result.Failure<Webhook>(error);
        }

        var mapped = CreateWebhook(webhookModel, "webhook create response");
        if (mapped.IsFailure(out var mappingError, out var webhook))
        {
            LogCreateWebhookError(mappingError.Message, mappingError);
            return Result.Failure<Webhook>(mappingError);
        }

        return Result.Success(webhook);
    }

    public async Task<Result<Webhook>> EditWebhookAsync(long id, WebhookEditParameters parameters, CancellationToken cancellationToken = default)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters), "The webhook edit parameters cannot be null.");

        var validationError = ValidationExtensions.ValidateWebhookId(id);
        if (validationError is not null)
            return Result.Failure<Webhook>(validationError);

        var mappedRequest = CreateWebhookRequest(parameters);
        if (mappedRequest.IsFailure(out var requestError, out var webhookRequest))
            return Result.Failure<Webhook>(requestError);

        Result<WebhookResponseModel> response;
        try
        {
            response = await postmark.PutAsync<WebhookRequestModel, WebhookResponseModel>(PostmarkTokenScope.Server, $"/webhooks/{id}", webhookRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogEditWebhookException(ex);
            return Result.Failure<Webhook>(ex);
        }

        if (response.IsFailure(out var error, out var webhookModel))
        {
            LogEditWebhookError(error.Message, error);
            return Result.Failure<Webhook>(error);
        }

        var mapped = CreateWebhook(webhookModel, "webhook edit response");
        if (mapped.IsFailure(out var mappingError, out var webhook))
        {
            LogEditWebhookError(mappingError.Message, mappingError);
            return Result.Failure<Webhook>(mappingError);
        }

        return Result.Success(webhook);
    }

    public async Task<Result<WebhookDeletion>> DeleteWebhookAsync(long id, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateWebhookId(id);
        if (validationError is not null)
            return Result.Failure<WebhookDeletion>(validationError);

        Result<WebhookDeletionModel> response;
        try
        {
            response = await postmark.DeleteAsync<WebhookDeletionModel>(PostmarkTokenScope.Server, $"/webhooks/{id}", cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDeleteWebhookException(ex);
            return Result.Failure<WebhookDeletion>(ex);
        }

        if (response.IsFailure(out var error, out var deletionModel))
        {
            LogDeleteWebhookError(error.Message, error);
            return Result.Failure<WebhookDeletion>(error);
        }

        var mapped = CreateWebhookDeletion(deletionModel);
        if (mapped.IsFailure(out var mappingError, out var deletion))
        {
            LogDeleteWebhookError(mappingError.Message, mappingError);
            return Result.Failure<WebhookDeletion>(mappingError);
        }

        return Result.Success(deletion);
    }

    private async Task<Result<WebhookList>> ListWebhooksCoreAsync(string? messageStream, CancellationToken cancellationToken)
    {
        Result<WebhookListModel> response;
        try
        {
            var endpoint = messageStream is null ? "/webhooks" : $"/webhooks?MessageStream={Uri.EscapeDataString(messageStream)}";
            response = await postmark.GetAsync<WebhookListModel>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogListWebhooksException(ex);
            return Result.Failure<WebhookList>(ex);
        }

        if (response.IsFailure(out var error, out var listModel))
        {
            LogListWebhooksError(error.Message, error);
            return Result.Failure<WebhookList>(error);
        }

        var mapped = CreateWebhookList(listModel);
        if (mapped.IsFailure(out var mappingError, out var webhookList))
        {
            LogListWebhooksError(mappingError.Message, mappingError);
            return Result.Failure<WebhookList>(mappingError);
        }

        return Result.Success(webhookList);
    }

    private static Result<WebhookRequestModel> CreateWebhookRequest(WebhookCreateParameters parameters)
    {
        var validationError = ValidationExtensions.ValidateWebhookUrl(parameters.Url, "The webhook create parameters URL");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookMessageStream(parameters.MessageStreamId, "The webhook create parameters message stream");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookHttpAuth(parameters.HttpAuth, "The webhook create parameters HTTP auth");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookHeaders(parameters.HttpHeaders, "The webhook create parameters HTTP headers");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookTriggers(parameters.Triggers, "The webhook create parameters triggers");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        return Result.Success(new WebhookRequestModel
        {
            Url = parameters.Url,
            MessageStream = parameters.MessageStreamId,
            HttpAuth = CreateWebhookHttpAuthModel(parameters.HttpAuth),
            HttpHeaders = CreateWebhookHeaderModels(parameters.HttpHeaders),
            Triggers = CreateWebhookTriggersModel(parameters.Triggers)
        });
    }

    private static Result<WebhookRequestModel> CreateWebhookRequest(WebhookEditParameters parameters)
    {
        if (parameters.Url is null && parameters.HttpAuth is null && parameters.HttpHeaders is null && parameters.Triggers is null)
            return Result.Failure<WebhookRequestModel>("The webhook edit parameters must set Url, HttpAuth, HttpHeaders, or Triggers.");

        var validationError = ValidationExtensions.ValidateWebhookUrl(parameters.Url, "The webhook edit parameters URL");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookHttpAuth(parameters.HttpAuth, "The webhook edit parameters HTTP auth");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookHeaders(parameters.HttpHeaders, "The webhook edit parameters HTTP headers");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        validationError = ValidationExtensions.ValidateWebhookTriggers(parameters.Triggers, "The webhook edit parameters triggers");
        if (validationError is not null)
            return Result.Failure<WebhookRequestModel>(validationError);

        return Result.Success(new WebhookRequestModel
        {
            Url = parameters.Url,
            HttpAuth = CreateWebhookHttpAuthModel(parameters.HttpAuth),
            HttpHeaders = CreateWebhookHeaderModels(parameters.HttpHeaders),
            Triggers = CreateWebhookTriggersModel(parameters.Triggers)
        });
    }

    private static WebhookHttpAuthModel? CreateWebhookHttpAuthModel(WebhookHttpAuth? auth)
    {
        return auth is null ? null : new WebhookHttpAuthModel { Username = auth.Username, Password = auth.Password };
    }

    private static List<WebhookHeaderModel>? CreateWebhookHeaderModels(IReadOnlyList<WebhookHeader>? headers)
    {
        return headers?.Select(static header => new WebhookHeaderModel { Name = header.Name, Value = header.Value })
            .ToList();
    }

    private static WebhookTriggersModel? CreateWebhookTriggersModel(WebhookTriggers? triggers)
    {
        return triggers is null
            ? null
            : new WebhookTriggersModel
            {
                Open = triggers.Open is null ? null : new WebhookOpenTriggerModel { Enabled = triggers.Open.Enabled, PostFirstOpenOnly = triggers.Open.PostFirstOpenOnly },
                Click = triggers.Click is null ? null : new WebhookBasicTriggerModel { Enabled = triggers.Click.Enabled },
                Delivery = triggers.Delivery is null ? null : new WebhookBasicTriggerModel { Enabled = triggers.Delivery.Enabled },
                Bounce = triggers.Bounce is null ? null : new WebhookContentTriggerModel { Enabled = triggers.Bounce.Enabled, IncludeContent = triggers.Bounce.IncludeContent },
                SpamComplaint = triggers.SpamComplaint is null ? null : new WebhookContentTriggerModel { Enabled = triggers.SpamComplaint.Enabled, IncludeContent = triggers.SpamComplaint.IncludeContent },
                SubscriptionChange = triggers.SubscriptionChange is null ? null : new WebhookBasicTriggerModel { Enabled = triggers.SubscriptionChange.Enabled }
            };
    }

    private static Result<WebhookList> CreateWebhookList(WebhookListModel response)
    {
        if (response.Webhooks is null)
            return Result.Failure<WebhookList>("Webhooks were not returned from the Postmark Webhooks API.");

        var webhooks = new List<Webhook>(response.Webhooks.Count);
        for (var index = 0; index < response.Webhooks.Count; index++)
        {
            var mapped = CreateWebhook(response.Webhooks[index], $"webhook list item {index}");
            if (mapped.IsFailure(out var error, out var webhook))
                return Result.Failure<WebhookList>(error);

            webhooks.Add(webhook);
        }

        return Result.Success(new WebhookList { Webhooks = webhooks });
    }

    private static Result<Webhook> CreateWebhook(WebhookResponseModel? response, string context)
    {
        if (response is null)
            return Result.Failure<Webhook>($"{context} returned from the Postmark Webhooks API was null.");

        if (response.Id is null)
            return Result.Failure<Webhook>($"ID was not returned from the Postmark Webhooks API for {context}.");

        if (response.Id.Value <= 0)
            return Result.Failure<Webhook>($"ID returned from the Postmark Webhooks API for {context} was invalid. Received {response.Id.Value}.");

        if (string.IsNullOrWhiteSpace(response.Url))
            return Result.Failure<Webhook>($"Url was not returned from the Postmark Webhooks API for {context}.");

        if (string.IsNullOrWhiteSpace(response.MessageStream))
            return Result.Failure<Webhook>($"MessageStream was not returned from the Postmark Webhooks API for {context}.");

        if (response.Triggers is null)
            return Result.Failure<Webhook>($"Triggers were not returned from the Postmark Webhooks API for {context}.");

        var auth = CreateWebhookHttpAuth(response.HttpAuth, context);
        if (auth.IsFailure(out var authError, out var mappedAuth))
            return Result.Failure<Webhook>(authError);

        var headers = CreateWebhookHeaders(response.HttpHeaders, context);
        if (headers.IsFailure(out var headersError, out var mappedHeaders))
            return Result.Failure<Webhook>(headersError);

        var triggers = CreateWebhookTriggers(response.Triggers, context);
        if (triggers.IsFailure(out var triggersError, out var mappedTriggers))
            return Result.Failure<Webhook>(triggersError);

        return Result.Success(new Webhook(response.Id.Value, response.Url, response.MessageStream, mappedAuth, mappedHeaders, mappedTriggers));
    }

    private static Result<WebhookHttpAuth?> CreateWebhookHttpAuth(WebhookHttpAuthModel? response, string context)
    {
        if (response is null)
            return Result.Success<WebhookHttpAuth?>(null);

        if (string.IsNullOrWhiteSpace(response.Username))
            return Result.Failure<WebhookHttpAuth?>($"HttpAuth.Username was not returned from the Postmark Webhooks API for {context}.");

        if (string.IsNullOrWhiteSpace(response.Password))
            return Result.Failure<WebhookHttpAuth?>($"HttpAuth.Password was not returned from the Postmark Webhooks API for {context}.");

        return Result.Success<WebhookHttpAuth?>(new WebhookHttpAuth { Username = response.Username, Password = response.Password });
    }

    private static Result<IReadOnlyList<WebhookHeader>> CreateWebhookHeaders(List<WebhookHeaderModel?>? response, string context)
    {
        if (response is null)
            return Result.Success<IReadOnlyList<WebhookHeader>>(Array.Empty<WebhookHeader>());

        var headers = new List<WebhookHeader>(response.Count);
        for (var index = 0; index < response.Count; index++)
        {
            var header = response[index];
            if (header is null)
                return Result.Failure<IReadOnlyList<WebhookHeader>>($"HttpHeaders item {index} returned from the Postmark Webhooks API for {context} was null.");

            if (string.IsNullOrWhiteSpace(header.Name))
                return Result.Failure<IReadOnlyList<WebhookHeader>>($"HttpHeaders item {index} returned from the Postmark Webhooks API for {context} did not include Name.");

            if (header.Value is null)
                return Result.Failure<IReadOnlyList<WebhookHeader>>($"HttpHeaders item {index} returned from the Postmark Webhooks API for {context} did not include Value.");

            headers.Add(new WebhookHeader { Name = header.Name, Value = header.Value });
        }

        return Result.Success<IReadOnlyList<WebhookHeader>>(headers);
    }

    private static Result<WebhookTriggers> CreateWebhookTriggers(WebhookTriggersModel response, string context)
    {
        var open = CreateOpenTrigger(response.Open, "Open", context);
        if (open.IsFailure(out var openError, out var mappedOpen))
            return Result.Failure<WebhookTriggers>(openError);

        var click = CreateBasicTrigger(response.Click, "Click", context);
        if (click.IsFailure(out var clickError, out var mappedClick))
            return Result.Failure<WebhookTriggers>(clickError);

        var delivery = CreateBasicTrigger(response.Delivery, "Delivery", context);
        if (delivery.IsFailure(out var deliveryError, out var mappedDelivery))
            return Result.Failure<WebhookTriggers>(deliveryError);

        var bounce = CreateContentTrigger(response.Bounce, "Bounce", context);
        if (bounce.IsFailure(out var bounceError, out var mappedBounce))
            return Result.Failure<WebhookTriggers>(bounceError);

        var spamComplaint = CreateContentTrigger(response.SpamComplaint, "SpamComplaint", context);
        if (spamComplaint.IsFailure(out var spamComplaintError, out var mappedSpamComplaint))
            return Result.Failure<WebhookTriggers>(spamComplaintError);

        var subscriptionChange = CreateBasicTrigger(response.SubscriptionChange, "SubscriptionChange", context);
        if (subscriptionChange.IsFailure(out var subscriptionChangeError, out var mappedSubscriptionChange))
            return Result.Failure<WebhookTriggers>(subscriptionChangeError);

        return Result.Success(new WebhookTriggers
        {
            Open = mappedOpen,
            Click = mappedClick,
            Delivery = mappedDelivery,
            Bounce = mappedBounce,
            SpamComplaint = mappedSpamComplaint,
            SubscriptionChange = mappedSubscriptionChange
        });
    }

    private static Result<WebhookBasicTrigger?> CreateBasicTrigger(WebhookBasicTriggerModel? response, string propertyName, string context)
    {
        if (response is null)
            return Result.Success<WebhookBasicTrigger?>(null);

        if (response.Enabled is null)
            return Result.Failure<WebhookBasicTrigger?>($"{propertyName}.Enabled was not returned from the Postmark Webhooks API for {context}.");

        return Result.Success<WebhookBasicTrigger?>(new WebhookBasicTrigger { Enabled = response.Enabled.Value });
    }

    private static Result<WebhookContentTrigger?> CreateContentTrigger(WebhookContentTriggerModel? response, string propertyName, string context)
    {
        if (response is null)
            return Result.Success<WebhookContentTrigger?>(null);

        if (response.Enabled is null)
            return Result.Failure<WebhookContentTrigger?>($"{propertyName}.Enabled was not returned from the Postmark Webhooks API for {context}.");

        if (response.IncludeContent is null)
            return Result.Failure<WebhookContentTrigger?>($"{propertyName}.IncludeContent was not returned from the Postmark Webhooks API for {context}.");

        return Result.Success<WebhookContentTrigger?>(new WebhookContentTrigger { Enabled = response.Enabled.Value, IncludeContent = response.IncludeContent.Value });
    }

    private static Result<WebhookOpenTrigger?> CreateOpenTrigger(WebhookOpenTriggerModel? response, string propertyName, string context)
    {
        if (response is null)
            return Result.Success<WebhookOpenTrigger?>(null);

        if (response.Enabled is null)
            return Result.Failure<WebhookOpenTrigger?>($"{propertyName}.Enabled was not returned from the Postmark Webhooks API for {context}.");

        if (response.PostFirstOpenOnly is null)
            return Result.Failure<WebhookOpenTrigger?>($"{propertyName}.PostFirstOpenOnly was not returned from the Postmark Webhooks API for {context}.");

        return Result.Success<WebhookOpenTrigger?>(new WebhookOpenTrigger { Enabled = response.Enabled.Value, PostFirstOpenOnly = response.PostFirstOpenOnly.Value });
    }

    private static Result<WebhookDeletion> CreateWebhookDeletion(WebhookDeletionModel response)
    {
        if (response.ErrorCode is null)
            return Result.Failure<WebhookDeletion>("ErrorCode was not returned from the Postmark Webhooks API.");

        if (string.IsNullOrWhiteSpace(response.Message))
            return Result.Failure<WebhookDeletion>("Message was not returned from the Postmark Webhooks API.");

        if (response.ErrorCode.Value != 0)
            return Result.Failure<WebhookDeletion>(new PostmarkError(response.ErrorCode.Value, response.Message));

        return Result.Success(new WebhookDeletion { Message = response.Message });
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to list webhooks.")]
    private partial void LogListWebhooksException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to list webhooks. {Message}")]
    private partial void LogListWebhooksError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to get the webhook.")]
    private partial void LogGetWebhookException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to get the webhook. {Message}")]
    private partial void LogGetWebhookError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to create the webhook.")]
    private partial void LogCreateWebhookException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to create the webhook. {Message}")]
    private partial void LogCreateWebhookError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to edit the webhook.")]
    private partial void LogEditWebhookException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to edit the webhook. {Message}")]
    private partial void LogEditWebhookError(string message, [LogProperties] IError error);

    [LoggerMessage(LogLevel.Error, "An exception occurred while attempting to delete the webhook.")]
    private partial void LogDeleteWebhookException(Exception ex);

    [LoggerMessage(LogLevel.Error, "Failed to delete the webhook. {Message}")]
    private partial void LogDeleteWebhookError(string message, [LogProperties] IError error);
}