using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using PostKit.BulkEmails;
using PostKit.Errors;

namespace PostKit.Tests;

public class PostKitClientBulkRequestTests
{
    private const string JobJson = """
        {"Id":"9859dd0b-1f4c-4a1e-8c2e-3d5b6a7c8e90","SubmittedAt":"2026-08-13T14:22:41Z",
         "Status":"Completed","TotalMessages":10,"PercentageCompleted":100,"Subject":"Newsletter","ReleasedCount":8,"FailedCount":2,"FutureField":{"Value":true}}
        """;

    [Fact]
    public async Task ListBulkEmailRequestsAsync_WithDefaults_MapsOnePageWithoutQueryParameters()
    {
        using var session = new PostmarkEndpointTestSession("{\"Requests\":[" + JobJson + "],\"PaginationKey\":\"opaque+/= key\"}");

        var result = await session.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        var request = Assert.Single(page.Requests);
        Assert.Equal(BulkEmailStatus.Completed, request.Status);
        Assert.Equal(8, request.ReleasedCount);
        Assert.Equal(2, request.FailedCount);
        Assert.Equal(8, request.AdditionalProperties!["ReleasedCount"].GetInt32());
        Assert.True(request.AdditionalProperties["FutureField"].GetProperty("Value").GetBoolean());
        Assert.Equal("opaque+/= key", page.PaginationKey);
        Assert.Throws<NotSupportedException>(() => ((IList<BulkEmailRequestSummary>)page.Requests).Clear());
        Assert.Equal(HttpMethod.Get, session.Method);
        Assert.Equal("/email/bulk", session.Endpoint);
        Assert.Equal("test-server-token", session.ServerToken);
        Assert.False(session.HasAccountToken);
    }

    [Theory]
    [InlineData(null, "/email/bulk?paginationKey=%20opaque%2B%2F%3D%20")]
    [InlineData(1, "/email/bulk?count=1&paginationKey=%20opaque%2B%2F%3D%20")]
    public async Task ListBulkEmailRequestsAsync_WithCursor_PreservesItsExactValue(int? count, string endpoint)
    {
        using var session = new PostmarkEndpointTestSession("""{"Requests":[]}""");

        var result = await session.Client.ListBulkEmailRequestsAsync(count, " opaque+/= ", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Empty(page.Requests);
        Assert.Null(page.PaginationKey);
        Assert.Equal(endpoint, session.Endpoint);
    }

    [Fact]
    public async Task ListBulkEmailRequestsAsync_WithMultiplePages_PassesTheReturnedCursorUntilCompletion()
    {
        using var session = new PostmarkEndpointTestSession("{\"Requests\":[" + JobJson + "],\"PaginationKey\":\" opaque+/= \"}",
            subsequentResponses: new[] { """{"Requests":[]}""" });

        var firstResult = await session.Client.ListBulkEmailRequestsAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(firstResult.IsSuccess(out var first), firstResult.ToString());
        var finalResult = await session.Client.ListBulkEmailRequestsAsync(1, first.PaginationKey, TestContext.Current.CancellationToken);

        Assert.True(finalResult.IsSuccess(out var final), finalResult.ToString());
        Assert.Single(first.Requests);
        Assert.Empty(final.Requests);
        Assert.Null(final.PaginationKey);
        Assert.Equal(2, session.RequestCount);
        Assert.Equal("/email/bulk?count=1&paginationKey=%20opaque%2B%2F%3D%20", session.Endpoint);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(-1, null)]
    [InlineData(null, "")]
    [InlineData(null, " \t")]
    public async Task ListBulkEmailRequestsAsync_WithInvalidPaging_FailsBeforeNetworkCall(int? count, string? cursor)
    {
        using var session = new PostmarkEndpointTestSession("""{"Requests":[]}""");

        var result = await session.Client.ListBulkEmailRequestsAsync(count, cursor, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Contains("null", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, session.RequestCount);
    }

    [Theory]
    [InlineData("Accepted", BulkEmailStatus.Accepted)]
    [InlineData("Processing", BulkEmailStatus.Processing)]
    [InlineData("Completed", BulkEmailStatus.Completed)]
    [InlineData("Failed", BulkEmailStatus.Failed)]
    [InlineData("Cancelled", BulkEmailStatus.Cancelled)]
    public async Task BulkResponses_WithKnownStatus_PreserveExistingAndNewValues(string status, BulkEmailStatus expected)
    {
        var job = JsonNode.Parse(JobJson)!;
        job["Status"] = status;
        using var listSession = new PostmarkEndpointTestSession("{\"Requests\":[" + job.ToJsonString() + "]}");
        using var statusSession = new PostmarkEndpointTestSession(job.ToJsonString());

        var listResult = await listSession.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);
        var statusResult = await statusSession.Client.GetBulkEmailStatusAsync(Guid.Parse("9859dd0b-1f4c-4a1e-8c2e-3d5b6a7c8e90"), TestContext.Current.CancellationToken);

        Assert.True(listResult.IsSuccess(out var page), listResult.ToString());
        Assert.True(statusResult.IsSuccess(out var details), statusResult.ToString());
        Assert.Equal(expected, Assert.Single(page.Requests).Status);
        Assert.Equal(expected, details.Status);
        Assert.Equal(8, details.ReleasedCount);
        Assert.Equal(2, details.FailedCount);
        Assert.Equal(8, details.AdditionalProperties!["ReleasedCount"].GetInt32());
    }

    [Fact]
    public void BulkEmailStatus_ExistingMembers_KeepTheirNumericValues()
    {
        Assert.Equal(0, (int)BulkEmailStatus.Accepted);
        Assert.Equal(1, (int)BulkEmailStatus.Processing);
        Assert.Equal(2, (int)BulkEmailStatus.Completed);
        Assert.Equal(3, (int)BulkEmailStatus.Failed);
        Assert.Equal(4, (int)BulkEmailStatus.Cancelled);
    }

    [Fact]
    public async Task ListBulkEmailRequestsAsync_WithNoSubject_PreservesTheAbsentValue()
    {
        var job = JsonNode.Parse(JobJson)!.AsObject();
        job.Remove("Subject");
        using var session = new PostmarkEndpointTestSession("{\"Requests\":[" + job.ToJsonString() + "]}");

        var result = await session.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Null(Assert.Single(page.Requests).Subject);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"eight\"")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    [InlineData("{}")]
    public async Task BulkResponses_WithUninterpretableCounters_PreserveRawDataWithoutRejectingTheJob(string value)
    {
        var job = JsonNode.Parse(JobJson)!;
        job["ReleasedCount"] = JsonNode.Parse(value);
        job["FailedCount"] = JsonNode.Parse(value);
        using var session = new PostmarkEndpointTestSession(job.ToJsonString());

        var result = await session.Client.GetBulkEmailStatusAsync(Guid.Parse("9859dd0b-1f4c-4a1e-8c2e-3d5b6a7c8e90"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var details), result.ToString());
        Assert.Null(details.ReleasedCount);
        Assert.Null(details.FailedCount);
        Assert.Equal(value, details.AdditionalProperties!["ReleasedCount"].GetRawText());
        Assert.Equal(value, details.AdditionalProperties["FailedCount"].GetRawText());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, JsonElement>)details.AdditionalProperties).Clear());
    }

    [Fact]
    public async Task BulkResponses_WithOlderPayloads_LeaveCountersAbsent()
    {
        var job = JsonNode.Parse(JobJson)!.AsObject();
        job.Remove("ReleasedCount");
        job.Remove("FailedCount");
        using var session = new PostmarkEndpointTestSession(job.ToJsonString());

        var result = await session.Client.GetBulkEmailStatusAsync(Guid.Parse("9859dd0b-1f4c-4a1e-8c2e-3d5b6a7c8e90"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var details), result.ToString());
        Assert.Null(details.ReleasedCount);
        Assert.Null(details.FailedCount);
        Assert.False(details.AdditionalProperties!.ContainsKey("ReleasedCount"));
    }

    [Theory]
    [InlineData("{\"Requests\":[null]}", "Requests[0]")]
    [InlineData("{\"Requests\":[{}]}", "Requests[0]")]
    [InlineData("{}", "Requests")]
    [InlineData("{\"Requests\":[],\"PaginationKey\":\" \"}", "PaginationKey")]
    [InlineData("{\"Requests\":{}}", "deserialized")]
    public async Task ListBulkEmailRequestsAsync_WithMalformedResponse_ReturnsActionableInvalidResponse(string body, string expected)
    {
        using var session = new PostmarkEndpointTestSession(body);

        var result = await session.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.IsType<PostmarkInvalidResponseError>(error);
        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(14, PostmarkErrorCode.EndpointApprovalRequired)]
    [InlineData(13, PostmarkErrorCode.InvalidPaginationKey)]
    [InlineData(99999, (PostmarkErrorCode)99999)]
    public async Task ListBulkEmailRequestsAsync_WithApiError_PreservesItsCode(int code, PostmarkErrorCode expected)
    {
        using var session = new PostmarkEndpointTestSession($"{{\"ErrorCode\":{code},\"Message\":\"Request rejected.\"}}", HttpStatusCode.UnprocessableEntity);

        var result = await session.Client.ListBulkEmailRequestsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal(expected, Assert.IsType<PostmarkError>(error).ErrorCode);
    }

    [Fact]
    public async Task ListBulkEmailRequestsAsync_WithCancellation_PropagatesCancellation()
    {
        using var session = new PostmarkEndpointTestSession("""{"Requests":[]}""");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.Client.ListBulkEmailRequestsAsync(cancellationToken: cancellation.Token));
    }
}
