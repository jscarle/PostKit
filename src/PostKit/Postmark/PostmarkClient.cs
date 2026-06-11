using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using LightResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Errors;
using PostKit.Postmark.Common;
using PostKit.Postmark.Email;

namespace PostKit.Postmark;

[UsedImplicitly]
internal sealed partial class PostmarkClient : IPostmarkClient
{
    private static readonly TimeSpan InitialTooManyRequestsRetryDelay = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan FinalTooManyRequestsRetryDelay = TimeSpan.FromMilliseconds(1600);
    private static readonly string Version = typeof(PostmarkClient).Assembly.GetName()
        .Version!.ToString(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostmarkClient> _logger;
    private readonly PostmarkRateLimiter _rateLimiter;

    public PostmarkClient(HttpClient httpClient, IOptions<PostKitOptions> options, ILogger<PostmarkClient> logger, PostmarkRateLimiter? rateLimiter = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _rateLimiter = rateLimiter ?? new PostmarkRateLimiter();

        if (string.IsNullOrWhiteSpace(options.Value.ServerApiToken))
            throw new InvalidOperationException("The server API token has not been set.");

        _httpClient.BaseAddress = new Uri("https://api.postmarkapp.com/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"{nameof(PostKit)} {Version}");
        _httpClient.DefaultRequestHeaders.Add("X-Postmark-Server-Token", options.Value.ServerApiToken);
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        var jsonToSend = JsonSerializer.Serialize(body, PostmarkConfiguration.JsonSerializerOptions);
        var sizeInBytes = Encoding.UTF8.GetByteCount(jsonToSend);
        using var responseMessage = await SendAsync(endpoint,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json) };
                return request;
            },
            sizeInBytes,
            cancellationToken
        );
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    public async Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await SendAsync(endpoint, () => new HttpRequestMessage(HttpMethod.Get, endpoint), null, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    public async Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        const string emptyJsonObject = "{}";
        using var responseMessage = await SendAsync(endpoint,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = new StringContent(emptyJsonObject, Encoding.UTF8, MediaTypeNames.Application.Json) };
                return request;
            },
            Encoding.UTF8.GetByteCount(emptyJsonObject),
            cancellationToken
        );
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(string endpoint, Func<HttpRequestMessage> createRequest, int? requestSizeInBytes, CancellationToken cancellationToken)
    {
        var nextTooManyRequestsRetryDelay = InitialTooManyRequestsRetryDelay;
        TimeSpan? previousTooManyRequestsRetryDelay = null;

        while (true)
        {
            var lease = await _rateLimiter.WaitForAvailabilityAsync(endpoint, cancellationToken);
            if (requestSizeInBytes.HasValue)
                LogApiRequest(endpoint, requestSizeInBytes.Value);

            using var request = createRequest();
            var responseMessage = await _httpClient.SendAsync(request, cancellationToken);
            _rateLimiter.ObserveResponse(endpoint, responseMessage.Headers, lease);

            if (responseMessage.StatusCode != HttpStatusCode.TooManyRequests)
                return responseMessage;

            if (previousTooManyRequestsRetryDelay.HasValue && previousTooManyRequestsRetryDelay.Value >= FinalTooManyRequestsRetryDelay)
                return responseMessage;

            responseMessage.Dispose();
            await _rateLimiter.DelayAsync(nextTooManyRequestsRetryDelay, cancellationToken);
            previousTooManyRequestsRetryDelay = nextTooManyRequestsRetryDelay;
            nextTooManyRequestsRetryDelay = TimeSpan.FromMilliseconds(Math.Min(nextTooManyRequestsRetryDelay.TotalMilliseconds * 2, FinalTooManyRequestsRetryDelay.TotalMilliseconds));
        }
    }

    private async Task<Result<TResponse>> GetResponse<TResponse>(string endpoint, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(endpoint, Encoding.UTF8.GetByteCount(receivedContent));
            TResponse? response;
            try
            {
                response = JsonSerializer.Deserialize<TResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);
            }
            catch (JsonException)
            {
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized because the response JSON was invalid.");
            }

            if (response is null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized because the response JSON was empty or did not match the expected shape.");

            return response;
        }

        return await GetRequestFailure<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    private async Task<Result<TResponse>> GetRequestFailure<TResponse>(string endpoint, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        var parsedErrorResponse = await TryGetPostmarkErrorResponse(endpoint, responseMessage, responseMessage.StatusCode == HttpStatusCode.UnprocessableEntity, cancellationToken);
        if (parsedErrorResponse.IsFailure(out var parseError, out var postmarkErrorResponse))
            return Result.Failure<TResponse>(parseError);

        if (postmarkErrorResponse is not null)
        {
            var postmarkError = new PostmarkError(responseMessage.StatusCode, postmarkErrorResponse);
            return Result.Failure<TResponse>(postmarkError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            var httpError = new HttpError(HttpStatusCode.Unauthorized, "The server API token is invalid.");
            return Result.Failure<TResponse>(httpError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            var httpError = new HttpError(HttpStatusCode.NotFound, $"The '{endpoint}' endpoint of the Postmark API could not be found.");
            return Result.Failure<TResponse>(httpError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            var httpError = new HttpError(HttpStatusCode.RequestEntityTooLarge, $"The payload for the request to the '{endpoint}' endpoint of the Postmark API was too large.");
            return Result.Failure<TResponse>(httpError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var httpError = new HttpError(HttpStatusCode.TooManyRequests, "The number of requests to the Postmark API has exceeded the rate limit.");
            return Result.Failure<TResponse>(httpError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.InternalServerError)
        {
            var httpError = new HttpError(HttpStatusCode.InternalServerError, $"An internal server error occurred while processing the request to the '{endpoint}' endpoint of the Postmark API.");
            return Result.Failure<TResponse>(httpError);
        }

        if (responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            var httpError = new HttpError(HttpStatusCode.ServiceUnavailable, "The Postmark API is currently unavailable.");
            return Result.Failure<TResponse>(httpError);
        }
        var genericHttpError = new HttpError(responseMessage.StatusCode, $"An '{(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}' error occurred while processing the request to the '{endpoint}' endpoint of the Postmark API."
        );
        return Result.Failure<TResponse>(genericHttpError);
    }

    private async Task<Result<PostmarkResponse?>> TryGetPostmarkErrorResponse(string endpoint, HttpResponseMessage responseMessage, bool strict, CancellationToken cancellationToken)
    {
        var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(receivedContent))
            return Result.Success<PostmarkResponse?>(null);

        LogApiResponse(endpoint, Encoding.UTF8.GetByteCount(receivedContent));

        try
        {
            var response = JsonSerializer.Deserialize<PostmarkResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);
            if (response is null && strict)
                return Result.Failure<PostmarkResponse?>($"The error response from the '{endpoint}' endpoint of the Postmark API could not be deserialized because the response JSON was empty or did not match the expected shape.");

            return Result.Success(response);
        }
        catch (JsonException) when (!strict)
        {
            return Result.Success<PostmarkResponse?>(null);
        }
        catch (JsonException)
        {
            return Result.Failure<PostmarkResponse?>($"The error response from the '{endpoint}' endpoint of the Postmark API could not be deserialized because the response JSON was invalid.");
        }
    }

    [LoggerMessage(LogLevel.Trace, "Postmark API request to {Endpoint} with {SizeInBytes} UTF-8 bytes.")]
    private partial void LogApiRequest(string endpoint, int sizeInBytes);

    [LoggerMessage(LogLevel.Trace, "Postmark API response from {Endpoint} with {SizeInBytes} UTF-8 bytes.")]
    private partial void LogApiResponse(string endpoint, int sizeInBytes);
}
