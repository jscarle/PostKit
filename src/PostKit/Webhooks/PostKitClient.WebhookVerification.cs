using System.Globalization;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Common;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Webhooks;
using PostKit.Webhooks;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<WebhookVerification>> VerifyWebhookAsync(long id, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateWebhookId(id);
        if (validationError is not null)
            return Result.Failure<WebhookVerification>(validationError);

        var endpoint = $"/webhooks/{id.ToString(CultureInfo.InvariantCulture)}/verify";
        Result<WebhookVerificationResponse> response;
        try
        {
            response = await postmark.PostAsync<WebhookVerificationResponse>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogWebhookVerificationException(exception);
            return Result.Failure<WebhookVerification>(exception);
        }

        if (response.IsFailure(out var error, out var model))
        {
            LogWebhookVerificationError(error.Message, error);
            return Result.Failure<WebhookVerification>(error);
        }

        var mapped = model.ToVerification(true);
        if (mapped.IsFailure(out var mappingError, out var verification))
        {
            var invalidResponse = new PostmarkInvalidResponseError(HttpMethod.Post, endpoint, mappingError.Message);
            LogWebhookVerificationError(invalidResponse.Message, invalidResponse);
            return Result.Failure<WebhookVerification>(invalidResponse);
        }

        return Result.Success(verification);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while verifying a webhook.")]
    private partial void LogWebhookVerificationException(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to verify a webhook. {Message}")]
    private partial void LogWebhookVerificationError(string message, [LogProperties] IError error);
}
