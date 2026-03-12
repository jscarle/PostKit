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
    private static readonly string Version = typeof(PostmarkClient).Assembly.GetName()
        .Version!.ToString(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostmarkClient> _logger;

    public PostmarkClient(HttpClient httpClient, IOptions<PostKitOptions> options, ILogger<PostmarkClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

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
        LogApiRequest(jsonToSend);
        using var contentToSend = new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var responseMessage = await _httpClient.PostAsync(endpoint, contentToSend, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    public async Task<Result<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await _httpClient.GetAsync(endpoint, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    public async Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
    {
        using var responseMessage = await _httpClient.PutAsync(endpoint, content: null, cancellationToken);
        return await GetResponse<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    private async Task<Result<TResponse>> GetResponse<TResponse>(string endpoint, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(receivedContent);
            var response = JsonSerializer.Deserialize<TResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);
            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            return response;
        }

        return await GetRequestFailure<TResponse>(endpoint, responseMessage, cancellationToken);
    }

    private async Task<Result<TResponse>> GetRequestFailure<TResponse>(string endpoint, HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        var parsedErrorResponse = await TryGetPostmarkErrorResponse(endpoint, responseMessage, strict: responseMessage.StatusCode == HttpStatusCode.UnprocessableEntity, cancellationToken);
        if (parsedErrorResponse.IsFailure(out var parseError, out var postmarkErrorResponse))
            return Result.Failure<TResponse>(parseError);

        if (postmarkErrorResponse is not null)
            return Result.Failure<TResponse>(new PostmarkError(responseMessage.StatusCode, postmarkErrorResponse));

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
        var genericHttpError = new HttpError(responseMessage.StatusCode, $"An '{(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}' error occurred while processing the request to the '{endpoint}' endpoint of the Postmark API.");
        return Result.Failure<TResponse>(genericHttpError);
    }

    private async Task<Result<PostmarkResponse?>> TryGetPostmarkErrorResponse(string endpoint, HttpResponseMessage responseMessage, bool strict, CancellationToken cancellationToken)
    {
        var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(receivedContent))
            return Result.Success<PostmarkResponse?>(null);

        LogApiResponse(receivedContent);

        try
        {
            var response = JsonSerializer.Deserialize<PostmarkResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);
            if (response is null && strict)
                return Result.Failure<PostmarkResponse?>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            return Result.Success<PostmarkResponse?>(response);
        }
        catch (JsonException) when (!strict)
        {
            return Result.Success<PostmarkResponse?>(null);
        }
        catch (JsonException)
        {
            return Result.Failure<PostmarkResponse?>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");
        }
    }

    [LoggerMessage(LogLevel.Trace, "Postmark API request: {Content}")]
    private partial void LogApiRequest(string content);

    [LoggerMessage(LogLevel.Trace, "Postmark API response: {Content}")]
    private partial void LogApiResponse(string content);
}
