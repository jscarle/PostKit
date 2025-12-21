using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using LightResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Common;
using PostKit.Configuration;
using PostKit.Errors;
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
        var contentToSend = new StringContent(jsonToSend, Encoding.UTF8, MediaTypeNames.Application.Json);
        using var responseMessage = await _httpClient.PostAsync(endpoint, contentToSend, cancellationToken);
        if (responseMessage.IsSuccessStatusCode)
        {
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(receivedContent);
            var response = JsonSerializer.Deserialize<TResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);
            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            return response;
        }

        if (responseMessage.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var receivedContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            LogApiResponse(receivedContent);
            var response = JsonSerializer.Deserialize<PostmarkResponse>(receivedContent, PostmarkConfiguration.JsonSerializerOptions);

            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            var postmarkError = new PostmarkError(response);
            return Result.Failure<TResponse>(postmarkError);
        }

        var httpError = new HttpError(responseMessage.StatusCode);
        return Result.Failure<TResponse>(httpError);
    }

    [LoggerMessage(LogLevel.Trace, "Postmark API request: {Content}")]
    private partial void LogApiRequest(string content);

    [LoggerMessage(LogLevel.Trace, "Postmark API response: {Content}")]
    private partial void LogApiResponse(string content);
}
