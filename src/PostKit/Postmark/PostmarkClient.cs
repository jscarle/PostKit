﻿using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Errors;
using PostKit.Postmark.Email;

namespace PostKit.Postmark;

internal sealed partial class PostmarkClient : IPostmarkClient
{
    private static readonly string Version = typeof(PostmarkClient).Assembly.GetName()
        .Version!.ToString(2);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostmarkClient> _logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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

    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
#if DEBUG
        var jsonToSend = JsonSerializer.Serialize(body, _jsonSerializerOptions);
        LogApiRequest(jsonToSend);
        var contentToSend = new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json);
        var responseMessage = await _httpClient.PostAsync(endpoint, contentToSend, cancellationToken);
#else
        var responseMessage = await _httpClient.PostAsJsonAsync(endpoint, body, _jsonSerializerOptions, cancellationToken);
#endif
        if (responseMessage.IsSuccessStatusCode)
        {
#if DEBUG
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(receivedContent);
            var response = JsonSerializer.Deserialize<TResponse>(receivedContent, _jsonSerializerOptions);
#else
            var response = await responseMessage.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
#endif
            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            return response;
        }

        if (responseMessage.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
#if DEBUG
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(receivedContent);
            var response = JsonSerializer.Deserialize<PostmarkResponse>(receivedContent, _jsonSerializerOptions);
#else
            var errorResponse = await responseMessage.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);
#endif

            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            var postmarkError = new PostmarkError(response);
            return Result.Failure<TResponse>(postmarkError);
        }

        var httpError = new HttpError(responseMessage.StatusCode);
        return Result.Failure<TResponse>(httpError);
    }

    [LoggerMessage(LogLevel.Information, "Postmark API request: {Content}")]
    private partial void LogApiRequest(string content);

    [LoggerMessage(LogLevel.Information, "Postmark API response: {Content}")]
    private partial void LogApiResponse(string content);
}
