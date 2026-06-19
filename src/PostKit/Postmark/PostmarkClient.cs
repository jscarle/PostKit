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
    private const int TooManyRequestsRetryCount = 6;
    private const double TooManyRequestsBackoffFactor = 2;
    private const double TooManyRequestsJitterRatio = 0.2;
    private static readonly TimeSpan InitialTooManyRequestsRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaximumTooManyRequestsRetryDelay = TimeSpan.FromSeconds(30);

    private static readonly string Version = typeof(PostmarkClient).Assembly.GetName()
        .Version!.ToString(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostmarkClient> _logger;

    private readonly IOptions<PostKitOptions> _options;
    private readonly PostmarkRateLimiter _rateLimiter;
    private readonly Func<double> _tooManyRequestsJitterMultiplierProvider;

    public PostmarkClient(HttpClient httpClient, IOptions<PostKitOptions> options, ILogger<PostmarkClient> logger, PostmarkRateLimiter? rateLimiter = null, Func<double>? tooManyRequestsJitterMultiplierProvider = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _rateLimiter = rateLimiter ?? new PostmarkRateLimiter();
        _tooManyRequestsJitterMultiplierProvider = tooManyRequestsJitterMultiplierProvider ?? GetRandomTooManyRequestsJitterMultiplier;

        if (!HasAnyApiToken(options.Value))
            throw new InvalidOperationException("At least one Postmark API token must be set. Set ServerApiToken for server-level endpoints or AccountApiToken for account-level endpoints.");

        _options = options;
        _httpClient.BaseAddress = new Uri("https://api.postmarkapp.com/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"{nameof(PostKit)} {Version}");
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TRequest, TResponse>(HttpMethod.Post, endpoint, body, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> PostAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        return await SendEmptyAsync<TResponse>(HttpMethod.Post, endpoint, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await SendAsync(endpoint, () => new HttpRequestMessage(HttpMethod.Get, endpoint), null, tokenScope, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> PutAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        if (tokenScope == PostmarkTokenScope.Account)
            return await SendEmptyAsync<TResponse>(HttpMethod.Put, endpoint, tokenScope, cancellationToken);

        const string emptyJsonObject = "{}";
        using var responseMessage = await SendAsync(endpoint, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint) { Content = new StringContent(emptyJsonObject, Encoding.UTF8, MediaTypeNames.Application.Json) };
            return request;
        }, Encoding.UTF8.GetByteCount(emptyJsonObject), tokenScope, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TRequest, TResponse>(HttpMethod.Put, endpoint, body, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        return await SendJsonAsync<TRequest, TResponse>(HttpMethod.Patch, endpoint, body, tokenScope, cancellationToken);
    }

    public async Task<Result<TResponse>> DeleteAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await SendAsync(endpoint, () => new HttpRequestMessage(HttpMethod.Delete, endpoint), null, tokenScope, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    private async Task<Result<TResponse>> SendJsonAsync<TRequest, TResponse>(HttpMethod method, string endpoint, TRequest body, PostmarkTokenScope tokenScope, CancellationToken cancellationToken)
    {
        var jsonToSend = JsonSerializer.Serialize(body, PostmarkConfiguration.JsonSerializerOptions);
        var sizeInBytes = Encoding.UTF8.GetByteCount(jsonToSend);
        using var responseMessage = await SendAsync(endpoint, () =>
        {
            var request = new HttpRequestMessage(method, endpoint) { Content = new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json) };
            return request;
        }, sizeInBytes, tokenScope, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    private async Task<Result<TResponse>> SendEmptyAsync<TResponse>(HttpMethod method, string endpoint, PostmarkTokenScope tokenScope, CancellationToken cancellationToken)
    {
        using var responseMessage = await SendAsync(endpoint, () =>
        {
            var request = new HttpRequestMessage(method, endpoint) { Content = new StringContent(string.Empty, Encoding.UTF8, MediaTypeNames.Application.Json) };
            return request;
        }, 0, tokenScope, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(string endpoint, Func<HttpRequestMessage> createRequest, int? requestSizeInBytes, PostmarkTokenScope tokenScope, CancellationToken cancellationToken)
    {
        var nextTooManyRequestsRetryDelay = InitialTooManyRequestsRetryDelay;
        var tooManyRequestsRetryCount = 0;

        while (true)
        {
            var lease = await _rateLimiter.WaitForAvailabilityAsync(endpoint, cancellationToken);
            if (requestSizeInBytes.HasValue)
                LogApiRequest(endpoint, requestSizeInBytes.Value);

            using var request = createRequest();
            AddTokenHeader(request, tokenScope);
            var responseMessage = await _httpClient.SendAsync(request, cancellationToken);
            _rateLimiter.ObserveResponse(endpoint, responseMessage.Headers, lease);

            if (responseMessage.StatusCode != HttpStatusCode.TooManyRequests)
                return responseMessage;

            if (tooManyRequestsRetryCount >= TooManyRequestsRetryCount)
                return responseMessage;

            responseMessage.Dispose();
            _rateLimiter.ObserveTooManyRequests(endpoint, ApplyTooManyRequestsJitter(nextTooManyRequestsRetryDelay));
            tooManyRequestsRetryCount++;
            nextTooManyRequestsRetryDelay = GetNextTooManyRequestsRetryDelay(nextTooManyRequestsRetryDelay);
        }
    }

    private void AddTokenHeader(HttpRequestMessage request, PostmarkTokenScope tokenScope)
    {
        var headerName = tokenScope == PostmarkTokenScope.Account ? "X-Postmark-Account-Token" : "X-Postmark-Server-Token";
        var token = tokenScope == PostmarkTokenScope.Account ? _options.Value.AccountApiToken : _options.Value.ServerApiToken;
        var tokenDescription = tokenScope == PostmarkTokenScope.Account ? "account" : "server";

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException($"The {tokenDescription} API token has not been set.");

        request.Headers.Add(headerName, token);
    }

    private static bool HasAnyApiToken(PostKitOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.ServerApiToken) || !string.IsNullOrWhiteSpace(options.AccountApiToken);
    }

    private TimeSpan ApplyTooManyRequestsJitter(TimeSpan delay)
    {
        var jitterMultiplier = Math.Clamp(_tooManyRequestsJitterMultiplierProvider(), 1 - TooManyRequestsJitterRatio, 1 + TooManyRequestsJitterRatio);
        var jitteredMilliseconds = Math.Ceiling(delay.TotalMilliseconds * jitterMultiplier);
        return TimeSpan.FromMilliseconds(Math.Min(jitteredMilliseconds, MaximumTooManyRequestsRetryDelay.TotalMilliseconds));
    }

    private static TimeSpan GetNextTooManyRequestsRetryDelay(TimeSpan currentDelay)
    {
        return TimeSpan.FromMilliseconds(Math.Min(currentDelay.TotalMilliseconds * TooManyRequestsBackoffFactor, MaximumTooManyRequestsRetryDelay.TotalMilliseconds));
    }

    private static double GetRandomTooManyRequestsJitterMultiplier()
    {
        return 1 - TooManyRequestsJitterRatio + Random.Shared.NextDouble() * TooManyRequestsJitterRatio * 2;
    }

    private async Task<Result<TResponse>> GetResponse<TResponse>(string endpoint, HttpResponseMessage responseMessage, PostmarkTokenScope tokenScope, CancellationToken cancellationToken)
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

        return await GetRequestFailure<TResponse>(endpoint, responseMessage, tokenScope, cancellationToken);
    }

    private async Task<Result<TResponse>> GetRequestFailure<TResponse>(string endpoint, HttpResponseMessage responseMessage, PostmarkTokenScope tokenScope, CancellationToken cancellationToken)
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
            var tokenDescription = tokenScope == PostmarkTokenScope.Account ? "account" : "server";
            var httpError = new HttpError(HttpStatusCode.Unauthorized, $"The {tokenDescription} API token is invalid.");
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

        var genericHttpError = new HttpError(responseMessage.StatusCode,
            $"An '{(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}' error occurred while processing the request to the '{endpoint}' endpoint of the Postmark API.");
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
