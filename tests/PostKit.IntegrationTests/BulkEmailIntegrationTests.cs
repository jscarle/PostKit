using System.Net;
using LightResults;
using PostKit.BulkEmails;
using PostKit.Common;
using PostKit.Errors;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for Postmark Bulk API support.</summary>
public class BulkEmailIntegrationTests
{
    private readonly IPostKitClient _client = CreateBulkClient();

    [Fact]
    public async Task SendBulkEmailAsync_WithDefaultBroadcastStreamAndCcOnlyMessage_Succeeds()
    {
        var bulkEmail = BulkEmail.CreateBuilder()
            .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
            .WithSubject("Bulk API default stream check")
            .WithTextBody("Hello from PostKit bulk.")
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentToEmail, nameof(TestConfiguration.DevelopmentToEmail)))
                .Build())
            .AddMessage(BulkEmailMessage.CreateBuilder()
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
        var bulkEmail = BulkEmail.CreateBuilder()
            .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
            .WithSubject("Bulk API personalized {{FirstName}}")
            .WithTextBody("Hi, {{FirstName}}")
            .UsingMessageStream(MessageStream.Broadcast)
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentToEmail, nameof(TestConfiguration.DevelopmentToEmail)))
                .WithTemplateModel(new { FirstName = "Alice" })
                .Build())
            .AddMessage(BulkEmailMessage.CreateBuilder()
                .To(RequireDevelopmentValue(TestConfiguration.DevelopmentCcEmail, nameof(TestConfiguration.DevelopmentCcEmail)))
                .WithTemplateModel(new { FirstName = "Bob" })
                .Build())
            .Build();

        var sendResult = await _client.SendBulkEmailAsync(bulkEmail, TestContext.Current.CancellationToken);
        var submitted = RequireBulkApi(sendResult);

        var status = await WaitForCompletionAsync(submitted.Id, TestContext.Current.CancellationToken);

        Assert.Equal(submitted.Id, status.Id);
        Assert.Equal(2, status.TotalMessages);
        Assert.Equal("Bulk API personalized {{FirstName}}", status.Subject);
        Assert.Equal(BulkEmailStatus.Completed, status.Status);
        Assert.InRange(status.PercentageCompleted, 100, 100);
    }

    private async Task<BulkEmailJob> WaitForCompletionAsync(Guid bulkRequestId, CancellationToken cancellationToken)
    {
        BulkEmailJob? lastStatus = null;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var result = await _client.GetBulkEmailStatusAsync(bulkRequestId, cancellationToken);
            lastStatus = RequireBulkApi(result);

            if (lastStatus.Status is BulkEmailStatus.Completed or BulkEmailStatus.Failed)
                break;

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        Assert.NotNull(lastStatus);
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
