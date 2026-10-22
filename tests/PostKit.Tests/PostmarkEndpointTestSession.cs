using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PostKit.Configuration;
using PostKit.Postmark;

namespace PostKit.Tests;

// Owns the complete HTTP test boundary so assertions exercise serialization, transport, and public mapping together.
internal sealed class PostmarkEndpointTestSession : IDisposable
{
    private readonly RecordingHandler _handler;
    private readonly HttpClient _httpClient;

    internal PostmarkEndpointTestSession(string response, HttpStatusCode status = HttpStatusCode.OK, string? errorCode = null, IReadOnlyList<string>? subsequentResponses = null)
    {
        _handler = new RecordingHandler(response, status, errorCode, subsequentResponses);
        _httpClient = new HttpClient(_handler);
        var options = Options.Create(new PostKitOptions { ServerApiToken = "test-server-token", AccountApiToken = "test-account-token" });
        var transport = new PostmarkClient(_httpClient, options, NullLogger<PostmarkClient>.Instance);
        Client = new PostKitClient(transport, NullLogger<PostKitClient>.Instance);
    }

    internal PostKitClient Client { get; }
    internal int RequestCount => _handler.RequestCount;
    internal HttpMethod? Method => _handler.Method;
    internal string? Endpoint => _handler.Endpoint;
    internal string? Body => _handler.Body;
    internal string? ServerToken => _handler.ServerToken;
    internal bool HasAccountToken => _handler.HasAccountToken;
    internal Exception? Failure { set => _handler.Failure = value; }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed class RecordingHandler(string response, HttpStatusCode status, string? errorCode, IReadOnlyList<string>? subsequentResponses) : HttpMessageHandler
    {
        internal int RequestCount { get; private set; }
        internal HttpMethod? Method { get; private set; }
        internal string? Endpoint { get; private set; }
        internal string? Body { get; private set; }
        internal string? ServerToken { get; private set; }
        internal bool HasAccountToken { get; private set; }
        internal Exception? Failure { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestCount++;
            Method = request.Method;
            Endpoint = request.RequestUri!.PathAndQuery;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            ServerToken = request.Headers.TryGetValues("X-Postmark-Server-Token", out var values) ? values.Single() : null;
            HasAccountToken = request.Headers.Contains("X-Postmark-Account-Token");
            if (Failure is not null)
                throw Failure;

            var responseBody = RequestCount > 1 && subsequentResponses is not null ? subsequentResponses[RequestCount - 2] : response;
            var message = new HttpResponseMessage(status) { Content = new StringContent(responseBody, Encoding.UTF8, "application/json") };
            if (errorCode is not null)
                message.Headers.Add("X-PM-ApiErrorCode", errorCode);
            return message;
        }
    }
}
