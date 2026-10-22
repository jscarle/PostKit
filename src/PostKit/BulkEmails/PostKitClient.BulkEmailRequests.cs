using System.Globalization;
using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.BulkEmails;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Bulk;

// ReSharper disable once CheckNamespace
namespace PostKit;

internal sealed partial class PostKitClient
{
    public async Task<Result<BulkEmailRequestPage>> ListBulkEmailRequestsAsync(int? count = null, string? paginationKey = null, CancellationToken cancellationToken = default)
    {
        if (count is <= 0)
            return Result.Failure<BulkEmailRequestPage>($"The bulk request count must be greater than zero. Received {count}. Pass null to use Postmark's default.");
        if (paginationKey is not null && string.IsNullOrWhiteSpace(paginationKey))
            return Result.Failure<BulkEmailRequestPage>("The bulk request paginationKey must not be empty or whitespace. Pass null to omit the pagination key.");

        var endpoint = "/email/bulk";
        if (count.HasValue)
            endpoint += $"?count={count.Value.ToString(CultureInfo.InvariantCulture)}";
        if (paginationKey is not null)
            endpoint += $"{(count.HasValue ? '&' : '?')}paginationKey={Uri.EscapeDataString(paginationKey)}";

        Result<BulkEmailListResponse> response;
        try
        {
            response = await postmark.GetAsync<BulkEmailListResponse>(PostmarkTokenScope.Server, endpoint, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            LogBulkEmailListException(exception);
            return Result.Failure<BulkEmailRequestPage>(exception);
        }

        if (response.IsFailure(out var error, out var model))
        {
            LogBulkEmailListError(error.Message, error);
            return Result.Failure<BulkEmailRequestPage>(error);
        }

        var mapped = CreateBulkEmailRequestPage(model);
        if (mapped.IsFailure(out var mappingError, out var page))
        {
            var invalidResponse = new PostmarkInvalidResponseError(HttpMethod.Get, endpoint, mappingError.Message);
            LogBulkEmailListError(invalidResponse.Message, invalidResponse);
            return Result.Failure<BulkEmailRequestPage>(invalidResponse);
        }

        return Result.Success(page);
    }

    private static Result<BulkEmailRequestPage> CreateBulkEmailRequestPage(BulkEmailListResponse response)
    {
        if (response.Requests is null)
            return Result.Failure<BulkEmailRequestPage>("Requests must be an array in the bulk request list response.");
        if (response.PaginationKey is not null && string.IsNullOrWhiteSpace(response.PaginationKey))
            return Result.Failure<BulkEmailRequestPage>("PaginationKey must be omitted or contain a nonempty cursor in the bulk request list response.");

        var requests = new BulkEmailRequestSummary[response.Requests.Count];
        for (var index = 0; index < response.Requests.Count; index++)
        {
            var request = response.Requests[index];
            if (request is null)
                return Result.Failure<BulkEmailRequestPage>($"Bulk Requests[{index}] must not be null.");

            var core = CreateBulkEmailResponseCore(request);
            if (core.IsFailure(out var error, out var mapped))
                return Result.Failure<BulkEmailRequestPage>($"Bulk Requests[{index}]: {error.Message}");
            if (mapped.Id == Guid.Empty)
                return Result.Failure<BulkEmailRequestPage>($"Bulk Requests[{index}].Id must not be an empty GUID.");
            if (request.TotalMessages is null or < 0)
                return Result.Failure<BulkEmailRequestPage>($"Bulk Requests[{index}].TotalMessages must be a nonnegative integer.");
            if (request.PercentageCompleted is null || !double.IsFinite(request.PercentageCompleted.Value) || request.PercentageCompleted is < 0 or > 100)
                return Result.Failure<BulkEmailRequestPage>($"Bulk Requests[{index}].PercentageCompleted must be a finite percentage between 0 and 100.");

            var additionalProperties = CloneAdditionalProperties(request.AdditionalProperties);
            var releasedCount = GetOptionalBulkEmailCount(request.AdditionalProperties, "ReleasedCount");
            var failedCount = GetOptionalBulkEmailCount(request.AdditionalProperties, "FailedCount");
            requests[index] = new BulkEmailRequestSummary(mapped.Id, mapped.Status, mapped.SubmittedAt, request.TotalMessages.Value, request.PercentageCompleted.Value,
                request.Subject, releasedCount, failedCount, additionalProperties);
        }

        var page = new BulkEmailRequestPage(Array.AsReadOnly(requests), response.PaginationKey);
        return Result.Success(page);
    }

    private static int? GetOptionalBulkEmailCount(Dictionary<string, JsonElement>? additionalProperties, string name)
    {
        if (additionalProperties is null)
            return null;

        if (!additionalProperties.TryGetValue(name, out var value))
        {
            foreach (var entry in additionalProperties)
            {
                if (!string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase))
                    continue;
                value = entry.Value;
                break;
            }
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var count) || count < 0)
            return null;
        return count;
    }

    [LoggerMessage(LogLevel.Error, "An exception occurred while listing bulk email requests.")]
    private partial void LogBulkEmailListException(Exception exception);

    [LoggerMessage(LogLevel.Error, "Failed to list bulk email requests. {Message}")]
    private partial void LogBulkEmailListError(string message, [LogProperties] IError error);
}
