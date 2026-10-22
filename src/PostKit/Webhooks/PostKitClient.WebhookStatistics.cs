using System.Collections.ObjectModel;
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
    public async Task<Result<WebhookStatistics>> GetWebhookStatisticsAsync(long id, CancellationToken cancellationToken = default)
    {
        var validationError = ValidationExtensions.ValidateWebhookId(id);
        if (validationError is not null)
            return Result.Failure<WebhookStatistics>(validationError);

        var endpoint = $"/webhooks/{id.ToString(CultureInfo.InvariantCulture)}/statistics";
        Result<WebhookStatisticsResponse> response;
        try
        {
            response = await postmark.GetAsync<WebhookStatisticsResponse>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogWebhookStatisticsException(exception);
            return Result.Failure<WebhookStatistics>(exception);
        }

        if (response.IsFailure(out var error, out var model))
        {
            LogWebhookStatisticsError(error.Message, error);
            return Result.Failure<WebhookStatistics>(error);
        }

        var mapped = CreateWebhookStatistics(model);
        if (mapped.IsFailure(out var mappingError, out var statistics))
        {
            var invalidResponse = new PostmarkInvalidResponseError(HttpMethod.Get, endpoint, mappingError.Message);
            LogWebhookStatisticsError(invalidResponse.Message, invalidResponse);
            return Result.Failure<WebhookStatistics>(invalidResponse);
        }

        return Result.Success(statistics);
    }

    private static Result<WebhookStatistics> CreateWebhookStatistics(WebhookStatisticsResponse response)
    {
        if (response.WebhookId is null or <= 0)
            return Result.Failure<WebhookStatistics>("WebhookId must be a positive identifier in the webhook statistics response.");
        if (response.ServerId is null or <= 0)
            return Result.Failure<WebhookStatistics>("ServerId must be a positive identifier in the webhook statistics response.");
        if (string.IsNullOrWhiteSpace(response.MessageStreamId) || string.IsNullOrWhiteSpace(response.Url))
            return Result.Failure<WebhookStatistics>("MessageStreamId and Url must be present in the webhook statistics response.");
        if (response.Statuses is null || response.MetricsByTrigger is null)
            return Result.Failure<WebhookStatistics>("Statuses and MetricsByTrigger must be objects in the webhook statistics response.");

        var range = response.TimeRange;
        if (range?.StartTime is null || range.EndTime is null || range.Hours is null or <= 0)
            return Result.Failure<WebhookStatistics>("TimeRange must contain StartTime, EndTime, and a positive Hours value.");
        if (range.StartTime > range.EndTime)
            return Result.Failure<WebhookStatistics>("TimeRange.StartTime must not be after EndTime.");

        var aggregateResult = CreateWebhookStatisticsMetrics(response.Metrics, "Metrics");
        if (aggregateResult.IsFailure(out var aggregateError, out var aggregate))
            return Result.Failure<WebhookStatistics>(aggregateError);

        var statuses = new Dictionary<string, string>(response.Statuses.Count, StringComparer.Ordinal);
        foreach (var entry in response.Statuses)
        {
            if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return Result.Failure<WebhookStatistics>($"Statuses contains an empty trigger name or status for '{entry.Key}'.");
            statuses.Add(entry.Key, entry.Value);
        }

        var metricsByTrigger = new Dictionary<string, WebhookStatisticsMetrics>(response.MetricsByTrigger.Count, StringComparer.Ordinal);
        foreach (var entry in response.MetricsByTrigger)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                return Result.Failure<WebhookStatistics>("MetricsByTrigger contains an empty trigger name.");
            var triggerResult = CreateWebhookStatisticsMetrics(entry.Value, $"MetricsByTrigger['{entry.Key}']");
            if (triggerResult.IsFailure(out var triggerError, out var metrics))
                return Result.Failure<WebhookStatistics>(triggerError);
            metricsByTrigger.Add(entry.Key, metrics);
        }

        var timeRange = new WebhookStatisticsTimeRange(range.StartTime.Value, range.EndTime.Value, range.Hours.Value);
        var statistics = new WebhookStatistics(response.WebhookId.Value, response.ServerId.Value, response.MessageStreamId, response.Url,
            new ReadOnlyDictionary<string, string>(statuses), timeRange, aggregate, new ReadOnlyDictionary<string, WebhookStatisticsMetrics>(metricsByTrigger));
        return Result.Success(statistics);
    }

    private static Result<WebhookStatisticsMetrics> CreateWebhookStatisticsMetrics(WebhookStatisticsMetricsResponse? response, string context)
    {
        if (response is null)
            return Result.Failure<WebhookStatisticsMetrics>($"{context} must contain webhook delivery metrics.");

        if (response.TotalRequests is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.TotalRequests must be a nonnegative integer.");
        if (response.SuccessCount is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.SuccessCount must be a nonnegative integer.");
        if (response.FailureCount is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.FailureCount must be a nonnegative integer.");
        if (response.RetryCount is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.RetryCount must be a nonnegative integer.");
        if (response.SlowCount is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.SlowCount must be a nonnegative integer.");
        if (response.VerySlowCount is null or < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.VerySlowCount must be a nonnegative integer.");
        if (response.AverageTerminalResponseTimeMs is < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.AverageTerminalResponseTimeMs must be null or a nonnegative integer.");
        if (response.AverageRetryResponseTimeMs is < 0)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.AverageRetryResponseTimeMs must be null or a nonnegative integer.");
        if (response.SuccessRate is null || !double.IsFinite(response.SuccessRate.Value) || response.SuccessRate is < 0 or > 100)
            return Result.Failure<WebhookStatisticsMetrics>($"{context}.SuccessRate must be a finite percentage between 0 and 100.");

        var metrics = new WebhookStatisticsMetrics(response.TotalRequests.Value, response.SuccessCount.Value, response.FailureCount.Value, response.RetryCount.Value,
            response.SuccessRate.Value, response.SlowCount.Value, response.VerySlowCount.Value, response.AverageTerminalResponseTimeMs, response.AverageRetryResponseTimeMs);
        return Result.Success(metrics);
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while getting webhook statistics.")]
    private partial void LogWebhookStatisticsException(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to get webhook statistics. {Message}")]
    private partial void LogWebhookStatisticsError(string message, [LogProperties] IError error);
}
