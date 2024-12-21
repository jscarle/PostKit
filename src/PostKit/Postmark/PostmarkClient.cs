using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightResults;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Errors;
using PostKit.Postmark.Email;

namespace PostKit.Postmark;

internal sealed class PostmarkClient : IPostmarkClient
{
    private static readonly string Version = typeof(PostmarkClient).Assembly.GetName()
        .Version!.ToString(2);

    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public PostmarkClient(HttpClient httpClient, IOptions<PostKitOptions> options)
    {
        _httpClient = httpClient;

        if (string.IsNullOrWhiteSpace(options.Value.ServerApiToken))
            throw new InvalidOperationException("The server API token has not been set.");

        _httpClient.BaseAddress = new Uri("https://api.postmarkapp.com/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"{nameof(PostKit)} {Version}");
        _httpClient.DefaultRequestHeaders.Add("X-Postmark-Server-Token", options.Value.ServerApiToken);
    }

    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken = default)
    {
        var responseMessage = await _httpClient.PostAsJsonAsync(endpoint, body, _jsonSerializerOptions, cancellationToken);
        if (responseMessage.IsSuccessStatusCode)
        {
            var response = await responseMessage.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

            if (response == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            return response;
        }

        if (responseMessage.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var errorResponse = await responseMessage.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken);

            if (errorResponse == null)
                return Result.Failure<TResponse>($"The response from the '{endpoint}' endpoint of the Postmark API could not be deserialized.");

            var postmarkError = new PostmarkError(errorResponse);
            return Result.Failure<TResponse>(postmarkError);
        }

        var httpError = new HttpError(responseMessage.StatusCode);
        return Result.Failure<TResponse>(httpError);
    }
}
