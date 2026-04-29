using System.Net;
using LightResults;
using MimeKit;
using PostKit.Bounces;
using PostKit.Common;
using PostKit.Emails;
using PostKit.Errors;

namespace PostKit.IntegrationTests;

/// <summary>Integration tests for Postmark Bounces API support.</summary>
public class BounceIntegrationTests
{
    private const string SoftBounceRecipient = "SoftBounce@bounce-testing.postmarkapp.com";
    private const string HardBounceRecipient = "HardBounce@bounce-testing.postmarkapp.com";
    private static readonly TimeSpan BounceSearchDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BounceDetailDelay = TimeSpan.FromSeconds(2);
    private readonly IPostKitClient _client = CreateBounceClient();

    [Fact]
    public async Task GetBouncesAsync_AfterSoftBounce_ReturnsMatchingBounce()
    {
        var sent = await SendSoftBounceAsync(TestContext.Current.CancellationToken);
        var searchResponse = await WaitForBounceAsync(sent.MessageId, false, TestContext.Current.CancellationToken);

        Assert.Equal(1, searchResponse.TotalCount);

        var bounce = Assert.Single(searchResponse.Bounces);
        Assert.Equal("Bounce", bounce.RecordType);
        Assert.Equal(sent.MessageId, bounce.MessageId);
        Assert.Equal(BounceType.SoftBounce, bounce.Type);
        Assert.Equal(SoftBounceRecipient, bounce.Email);
        Assert.Equal(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)), bounce.From);
        Assert.Equal("outbound", bounce.MessageStream);
        Assert.True(bounce.DumpAvailable);
        Assert.False(bounce.Inactive);
        Assert.True(bounce.CanActivate);
        Assert.Contains("smtp;", bounce.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetBounceAsync_AfterSoftBounce_ReturnsRawContent()
    {
        var sent = await SendSoftBounceAsync(TestContext.Current.CancellationToken);
        var searchResponse = await WaitForBounceAsync(sent.MessageId, false, TestContext.Current.CancellationToken);
        var bounce = Assert.Single(searchResponse.Bounces);

        var detail = await WaitForBounceDetailsAsync(bounce.Id, response => response.MessageId == sent.MessageId && response.Content.Contains($"X-PM-Message-Id: {sent.MessageId:D}", StringComparison.Ordinal),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(bounce.Id, detail.Id);
        Assert.Equal(sent.MessageId, detail.MessageId);
        Assert.Equal(bounce.Email, detail.Email);
        Assert.Equal(bounce.From, detail.From);
        Assert.Equal(bounce.Subject, detail.Subject);
        Assert.Contains($"X-PM-Message-Id: {sent.MessageId:D}", detail.Content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetBounceDumpAsync_AfterSoftBounce_ReturnsRawBody()
    {
        var sent = await SendSoftBounceAsync(TestContext.Current.CancellationToken);
        var searchResponse = await WaitForBounceAsync(sent.MessageId, false, TestContext.Current.CancellationToken);
        var bounce = Assert.Single(searchResponse.Bounces);

        var dump = await WaitForBounceDumpAsync(bounce.Id, response => response.Body.Contains($"X-PM-Message-Id: {sent.MessageId:D}", StringComparison.Ordinal), TestContext.Current.CancellationToken);

        Assert.Contains($"X-PM-Message-Id: {sent.MessageId:D}", dump.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDeliveryStatsAsync_AfterSoftBounce_ReturnsCounts()
    {
        var sent = await SendSoftBounceAsync(TestContext.Current.CancellationToken);
        await WaitForBounceAsync(sent.MessageId, false, TestContext.Current.CancellationToken);

        var stats = await WaitForDeliveryStatsAsync(TestContext.Current.CancellationToken);

        Assert.True(stats.InactiveMails >= 0);
        Assert.NotEmpty(stats.Bounces);
        Assert.Contains(stats.Bounces, item => item is { Name: "All", Type: null, Count: >= 1 });
        Assert.Contains(stats.Bounces, item => item is { Type: BounceType.SoftBounce, Count: >= 1 });
    }

    [Fact]
    public async Task ActivateBounceAsync_AfterHardBounce_ReactivatesRecipient()
    {
        var bounce = await EnsureInactiveHardBounceAsync(TestContext.Current.CancellationToken);

        var activationResult = await _client.ActivateBounceAsync(bounce.Id, TestContext.Current.CancellationToken);
        var activation = RequireBounceApi(activationResult);

        Assert.Equal("OK", activation.Message);
        Assert.Equal(bounce.Id, activation.Bounce.Id);
        Assert.Equal(BounceType.HardBounce, activation.Bounce.Type);
        Assert.False(activation.Bounce.Inactive);

        var reactivated = await WaitForBounceStateAsync(bounce.Id, false, TestContext.Current.CancellationToken);
        Assert.False(reactivated.Inactive);
    }

    private async Task<EmailSubmission> SendSoftBounceAsync(CancellationToken cancellationToken)
    {
        return await SendBounceAsync(SoftBounceRecipient, "Generate a fake soft bounce for Bounces API integration testing.", cancellationToken);
    }

    private async Task<EmailSubmission> SendBounceAsync(string recipient, string textBody, CancellationToken cancellationToken)
    {
        var subject = $"PostKit bounce integration {Guid.NewGuid():N}";
        var email = Email.Compose()
            .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
            .To(recipient)
            .Subject(subject)
            .TextBody(textBody)
            .UseMessageStream(MessageStream.Transactional)
            .Build();

        var result = await _client.SendEmailAsync(email, cancellationToken);
        return RequireSendResult(result);
    }

    private async Task<BouncePage> WaitForBounceAsync(Guid messageId, bool inactive, CancellationToken cancellationToken)
    {
        BouncePage? lastResponse = null;

        for (var attempt = 0; attempt < 24; attempt++)
        {
            var result = await _client.GetBouncesAsync(new BounceQuery
                {
                    Count = 10, MessageId = messageId, Inactive = inactive, MessageStream = "outbound",
                }, cancellationToken
            );

            lastResponse = RequireBounceApi(result);
            if (lastResponse.TotalCount > 0)
                break;

            await Task.Delay(BounceSearchDelay, cancellationToken);
        }

        Assert.NotNull(lastResponse);
        return lastResponse;
    }

    private Task<BounceDetails> WaitForBounceDetailsAsync(long id, Func<BounceDetails, bool> isReady, CancellationToken cancellationToken)
    {
        return WaitForBounceApiAsync(ct => _client.GetBounceAsync(id, ct), isReady, $"Bounce '{id}' details were not ready after waiting for replication.", cancellationToken);
    }

    private Task<BounceDump> WaitForBounceDumpAsync(long id, Func<BounceDump, bool> isReady, CancellationToken cancellationToken)
    {
        return WaitForBounceApiAsync(ct => _client.GetBounceDumpAsync(id, ct), isReady, $"Bounce '{id}' dump was not ready after waiting for replication.", cancellationToken);
    }

    private async Task<DeliveryStats> WaitForDeliveryStatsAsync(CancellationToken cancellationToken)
    {
        DeliveryStats? lastResponse = null;

        for (var attempt = 0; attempt < 6; attempt++)
        {
            var result = await _client.GetDeliveryStatsAsync(cancellationToken);
            lastResponse = RequireBounceApi(result);

            if (lastResponse.Bounces.Any(item => item is { Type: BounceType.SoftBounce, Count: >= 1 }))
                break;

            await Task.Delay(BounceSearchDelay, cancellationToken);
        }

        Assert.NotNull(lastResponse);
        return lastResponse;
    }

    private async Task<Bounce> EnsureInactiveHardBounceAsync(CancellationToken cancellationToken)
    {
        var existing = await FindHardBounceAsync(true, cancellationToken);
        if (existing is not null)
            return existing;

        var sendResult = await _client.SendEmailAsync(Email.Compose()
                .From(RequireDevelopmentValue(TestConfiguration.DevelopmentFromEmail, nameof(TestConfiguration.DevelopmentFromEmail)))
                .To(HardBounceRecipient)
                .Subject($"PostKit hard bounce activation {Guid.NewGuid():N}")
                .TextBody("Generate a hard bounce that can be reactivated.")
                .UseMessageStream(MessageStream.Transactional)
                .Build(), cancellationToken
        );

        if (sendResult.IsSuccess(out var sent))
        {
            var searchResponse = await WaitForBounceAsync(sent.MessageId, true, cancellationToken);
            return Assert.Single(searchResponse.Bounces);
        }

        Assert.True(sendResult.IsFailure(out var sendError, out var _), sendResult.ToString());

        if (sendError.Message.Contains("inactive", StringComparison.OrdinalIgnoreCase))
        {
            existing = await FindHardBounceAsync(true, cancellationToken);
            if (existing is not null)
                return existing;
        }

        Assert.Fail(sendResult.ToString());
        return null!;
    }

    private async Task<Bounce?> FindHardBounceAsync(bool inactive, CancellationToken cancellationToken)
    {
        var result = await _client.GetBouncesAsync(new BounceQuery
            {
                Count = 1,
                Type = BounceType.HardBounce,
                Inactive = inactive,
                EmailFilter = new MailboxAddress(string.Empty, HardBounceRecipient),
                MessageStream = "outbound",
            }, cancellationToken
        );

        var response = RequireBounceApi(result);
        return response.Bounces.FirstOrDefault();
    }

    private async Task<BounceDetails> WaitForBounceStateAsync(long id, bool inactive, CancellationToken cancellationToken)
    {
        return await WaitForBounceDetailsAsync(id, response => response.Inactive == inactive, cancellationToken);
    }

    private static EmailSubmission RequireSendResult(Result<EmailSubmission> result)
    {
        if (result.IsSuccess(out var response))
            return response;

        Assert.Fail(result.ToString());
        return null!;
    }

    private static T RequireBounceApi<T>(Result<T> result)
        where T : class
    {
        if (result.IsSuccess(out var response))
            return response;

        Assert.True(result.IsFailure(out var error, out var _), result.ToString());

        if (ShouldSkip(error))
            Assert.Skip($"Bounces API is not available in this environment: {error.Message}");

        Assert.Fail(result.ToString());
        return null!;
    }

    private static IPostKitClient CreateBounceClient()
    {
        var apiToken = RequireDevelopmentValue(TestConfiguration.DevelopmentApiToken, nameof(TestConfiguration.DevelopmentApiToken));
        return TestHelper.CreateClient(apiToken);
    }

    private static string RequireDevelopmentValue(string? value, string settingName)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        Assert.Skip($"Bounces API integration tests require {settingName} in appsettings.Development.json.");
        return null!;
    }

    private static bool ShouldSkip(IError error)
    {
        if (error is HttpError { StatusCode: HttpStatusCode.NotFound })
            return true;

        return error.Message.Contains("requires activation", StringComparison.OrdinalIgnoreCase)
               || (error.Message.Contains("bounces api", StringComparison.OrdinalIgnoreCase) && error.Message.Contains("activation", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldRetryBounceLookup(IError error)
    {
        return error is PostmarkError { ErrorCode: PostmarkErrorCode.BounceNotFound };
    }

    private static async Task<T> WaitForBounceApiAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, Func<T, bool> isReady, string timeoutMessage, CancellationToken cancellationToken)
        where T : class
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var result = await operation(cancellationToken);
            if (result.IsSuccess(out var response))
            {
                if (isReady(response))
                    return response;
            }
            else
            {
                Assert.True(result.IsFailure(out var error, out var _), result.ToString());

                if (ShouldSkip(error))
                    Assert.Skip($"Bounces API is not available in this environment: {error.Message}");

                if (!ShouldRetryBounceLookup(error))
                    Assert.Fail(result.ToString());
            }

            await Task.Delay(BounceDetailDelay, cancellationToken);
        }

        Assert.Fail(timeoutMessage);
        return null!;
    }
}
