using System.Net;
using LightResults;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Errors;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for Postmark Bulk API support.</summary>
public class BulkEmailIntegrationTests
{
    private static readonly TimeSpan BulkStatusDelay = TimeSpan.FromSeconds(2);
    private readonly IPostKitClient _client = CreateBulkClient();

    [Fact]
    public async Task SendBulkEmailAsync_WithDefaultBroadcastStreamAndCcOnlyMessage_Succeeds()
    {
        var bulkEmail = BulkEmail.Compose()
            .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
            .Subject("Bulk API default stream check")
            .TextBody("Hello from PostKit bulk.")
            .AddMessage(BulkEmailMessage.Compose()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentToEmail, nameof(TestConfiguration.DevelopmentToEmail)))
                .Build())
            .AddMessage(BulkEmailMessage.Compose()
                .Cc(RequireDevelopmentValue(TestConfiguration.DevelopmentCcEmail, nameof(TestConfiguration.DevelopmentCcEmail)))
                .Build())
            .Build();

        var result = await _client.SendBulkEmailAsync(bulkEmail, TestContext.Current.CancellationToken);
        var response = RequireBulkApi(result);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(2, response.TotalMessages);
        Assert.Equal("Bulk API default stream check", response.Subject);
        Assert.InRange(response.PercentageCompleted, 0, 100);
        Assert.True(response.Status is BulkEmailStatus.Accepted or BulkEmailStatus.Processing or BulkEmailStatus.Completed);
    }

    [Fact]
    public async Task GetBulkEmailStatusAsync_AfterSubmission_Completes()
    {
        var bulkEmail = BulkEmail.Compose()
            .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
            .Subject("Bulk API status tracking")
            .TextBody("Hi from PostKit bulk.")
            .UseMessageStream(MessageStream.Broadcast)
            .AddMessage(BulkEmailMessage.Compose()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentToEmail, nameof(TestConfiguration.DevelopmentToEmail)))
                .Build())
            .AddMessage(BulkEmailMessage.Compose()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentCcEmail, nameof(TestConfiguration.DevelopmentCcEmail)))
                .Build())
            .Build();

        var sendResult = await _client.SendBulkEmailAsync(bulkEmail, TestContext.Current.CancellationToken);
        var submitted = RequireBulkApi(sendResult);

        var status = await WaitForCompletionAsync(submitted.Id, TestContext.Current.CancellationToken);

        Assert.Equal(submitted.Id, status.Id);
        Assert.Equal(2, status.TotalMessages);
        Assert.Equal("Bulk API status tracking", status.Subject);
        Assert.Equal(BulkEmailStatus.Completed, status.Status);
        Assert.InRange(status.PercentageCompleted, 100, 100);
    }

    private async Task<BulkEmailJob> WaitForCompletionAsync(Guid bulkRequestId, CancellationToken cancellationToken)
    {
        BulkEmailJob? lastStatus = null;

        for (var attempt = 0; attempt < 30; attempt++)
        {
            var result = await _client.GetBulkEmailStatusAsync(bulkRequestId, cancellationToken);
            lastStatus = RequireBulkApi(result);

            if (lastStatus.Status is BulkEmailStatus.Completed or BulkEmailStatus.Failed)
                break;

            await Task.Delay(BulkStatusDelay, cancellationToken);
        }

        Assert.NotNull(lastStatus);
        Assert.True(
            lastStatus.Status is BulkEmailStatus.Completed or BulkEmailStatus.Failed,
            $"Bulk request '{bulkRequestId:D}' remained in '{lastStatus.Status}' after waiting."
        );
        return lastStatus;
    }

    private static BulkEmailJob RequireBulkApi(Result<BulkEmailJob> result)
    {
        if (result.IsSuccess(out var response))
            return response;

        Assert.True(result.IsFailure(out var error, out BulkEmailJob? _), result.ToString());

        if (ShouldSkip(error))
            Assert.Skip($"Bulk API is not available in this environment: {error.Message}");

        Assert.Fail(result.ToString());
        return null!;
    }

    private static IPostKitClient CreateBulkClient()
    {
        var apiToken = RequireDevelopmentValue(TestConfiguration.DevelopmentApiToken, nameof(TestConfiguration.DevelopmentApiToken));
        return TestHelper.CreateClient(apiToken);
    }

    private static string RequireDevelopmentValue(string? value, string settingName)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        Assert.Skip($"Bulk API integration tests require {settingName} in appsettings.Development.json.");
        return null!;
    }

    private static bool ShouldSkip(IError error)
    {
        if (error is HttpError { StatusCode: HttpStatusCode.NotFound })
            return true;

        return error.Message.Contains("requires activation", StringComparison.OrdinalIgnoreCase)
               || error.Message.Contains("bulk api", StringComparison.OrdinalIgnoreCase)
               && error.Message.Contains("activation", StringComparison.OrdinalIgnoreCase);
    }
}
