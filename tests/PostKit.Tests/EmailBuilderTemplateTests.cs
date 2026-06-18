using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Emails;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.Postmark.Email;

namespace PostKit.Tests;

public class EmailBuilderTemplateTests
{
    [Fact]
    public void UsingTemplateId_WithTemplateModel_Build_SetsTemplateProperties()
    {
        var templateModel = new { Name = "Alice" };

        var email = Email.FromTemplate(42, true)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel)
            .Build();
        var builtTemplateModel = Assert.IsType<JsonObject>(email.TemplateModel);

        Assert.Equal(42, email.TemplateId);
        Assert.Null(email.TemplateAlias);
        Assert.Equal("Alice", builtTemplateModel["name"]!.GetValue<string>());
        Assert.True(email.InlineCss);
    }

    [Fact]
    public void UsingTemplateAlias_WithTemplateModel_Build_SetsTemplateProperties()
    {
        var templateModel = new { Name = "Bob" };

        var email = Email.FromTemplate("welcome-email", false)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel)
            .Build();
        var builtTemplateModel = Assert.IsType<JsonObject>(email.TemplateModel);

        Assert.Null(email.TemplateId);
        Assert.Equal("welcome-email", email.TemplateAlias);
        Assert.Equal("Bob", builtTemplateModel["name"]!.GetValue<string>());
        Assert.False(email.InlineCss);
    }

    [Fact]
    public void FromTemplate_WithInvalidTemplateId_ThrowsHelpfulExceptionWithActualValue()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.FromTemplate(0));

        Assert.Equal("templateId", exception.ParamName);
        Assert.Equal("The template ID must be greater than zero. Received 0. (Parameter 'templateId')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithUnserializableModel_ThrowsArgumentException()
    {
        var templateModel = CyclicTemplateModel.Create();

        var exception = Assert.Throws<ArgumentException>(() => Email.FromTemplate(7)
            .WithModel(templateModel));

        Assert.Equal("The template model could not be serialized to a JSON object. Ensure it does not contain cycles or members unsupported by System.Text.Json. (Parameter 'templateModel')", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void WithTemplateModel_WithScalarModel_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Email.FromTemplate(7)
            .WithModel("Alice"));

        Assert.Equal("The template model must serialize to a JSON object. (Parameter 'templateModel')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithGlobalSerializerOptions_UsesConfiguredNamingPolicy()
    {
        var previousOptions = PostKitTemplateModelSerialization.DefaultSerializerOptions;

        try
        {
            PostKitTemplateModelSerialization.DefaultSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var email = Email.FromTemplate(7)
                .From("sender@postkit.com")
                .To("recipient@postkit.com")
                .WithModel(new { FirstName = "Alice" })
                .Build();

            var request = email.ToEmailRequest();

            Assert.NotNull(request.TemplateModel);
            Assert.Equal("Alice", request.TemplateModel["FirstName"]!.GetValue<string>());
            Assert.Null(request.TemplateModel["firstName"]);
        }
        finally
        {
            PostKitTemplateModelSerialization.DefaultSerializerOptions = previousOptions;
        }
    }

    [Fact]
    public void DefaultSerializerOptions_WithNullValue_ThrowsHelpfulException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => PostKitTemplateModelSerialization.DefaultSerializerOptions = null!);

        Assert.Equal("value", exception.ParamName);
        Assert.Equal("Default serializer options cannot be null. (Parameter 'value')", exception.Message);
    }

    [Fact]
    public void WithTemplateModel_WithPerCallSerializerOptions_OverridesGlobalDefaults()
    {
        var previousOptions = PostKitTemplateModelSerialization.DefaultSerializerOptions;

        try
        {
            PostKitTemplateModelSerialization.DefaultSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var builder = Email.FromTemplate(7)
                .From("sender@postkit.com")
                .To("recipient@postkit.com");

            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            var email = builder.WithModel(new { FirstName = "Alice" }, serializerOptions)
                .Build();

            var request = email.ToEmailRequest();

            Assert.NotNull(request.TemplateModel);
            Assert.Equal("Alice", request.TemplateModel["FirstName"]!.GetValue<string>());
            Assert.Null(request.TemplateModel["firstName"]);
        }
        finally
        {
            PostKitTemplateModelSerialization.DefaultSerializerOptions = previousOptions;
        }
    }

    [Fact]
    public async Task SendEmailAsync_WithTemplate_RoutesThroughTemplateEndpoint()
    {
        var templateModel = new { Name = "Charlie" };

        var email = Email.FromTemplate(7)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel)
            .Build();

        var postmark = new RecordingPostmarkClient();
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess());
        Assert.Equal("/email/withTemplate", postmark.LastEndpoint);
        Assert.IsType<EmailRequest>(postmark.LastRequest);
    }

    [Fact]
    public async Task SendEmailAsync_MapsPostmarkResponseFields()
    {
        var messageId = Guid.Parse("0b261aa1-6726-4d7f-8ead-13ba17bc8283");
        var submittedAt = new DateTimeOffset(2026, 3, 10, 22, 33, 1, TimeSpan.Zero);
        var email = Email.FromTemplate(7)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(new { Name = "Delta" })
            .Build();

        var postmark = new RecordingPostmarkClient(new EmailResponse
        {
            MessageId = messageId.ToString("D"),
            To = "recipient@postkit.com",
            SubmittedAt = submittedAt,
            ErrorCode = 0,
            Message = "OK"
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal(messageId, response.MessageId);
        Assert.Equal("<0b261aa1-6726-4d7f-8ead-13ba17bc8283@mtasv.net>", response.InternetMessageId);
        Assert.Equal("recipient@postkit.com", response.To);
        Assert.Equal(submittedAt, response.SubmittedAt);
    }

    [Fact]
    public async Task SendEmailAsync_WithKeepIdAndMessageIdHeader_UsesHeaderForInternetMessageId()
    {
        var messageId = Guid.Parse("0b261aa1-6726-4d7f-8ead-13ba17bc8283");
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Preserve Message-ID")
            .TextBody("Hello")
            .AddHeader("Message-ID", "<custom@example.com>")
            .AddHeader("X-PM-KeepID", "true")
            .Build();

        var postmark = new RecordingPostmarkClient(new EmailResponse
        {
            MessageId = messageId.ToString("D"),
            To = "recipient@postkit.com",
            SubmittedAt = DateTimeOffset.UtcNow,
            ErrorCode = 0,
            Message = "OK"
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("<custom@example.com>", response.InternetMessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithMessageIdHeaderButWithoutKeepId_FallsBackToPostmarkInternetMessageId()
    {
        var messageId = Guid.Parse("0b261aa1-6726-4d7f-8ead-13ba17bc8283");
        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Replace Message-ID")
            .TextBody("Hello")
            .AddHeader("Message-ID", "<custom@example.com>")
            .Build();

        var postmark = new RecordingPostmarkClient(new EmailResponse
        {
            MessageId = messageId.ToString("D"),
            To = "recipient@postkit.com",
            SubmittedAt = DateTimeOffset.UtcNow,
            ErrorCode = 0,
            Message = "OK"
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsSuccess(out var response), result.ToString());
        Assert.Equal("<0b261aa1-6726-4d7f-8ead-13ba17bc8283@mtasv.net>", response.InternetMessageId);
    }

    [Fact]
    public void ResolveInternetMessageId_WithoutKeepIdRequirement_UsesMessageIdHeader()
    {
        var messageId = Guid.Parse("0b261aa1-6726-4d7f-8ead-13ba17bc8283");
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Message-ID"] = "<custom@example.com>" };

        var internetMessageId = EmailSubmission.ResolveInternetMessageId(messageId, headers, false);

        Assert.Equal("<custom@example.com>", internetMessageId);
    }

    [Fact]
    public async Task SendEmailAsync_WithoutSubmittedAt_ReturnsFailure()
    {
        var email = Email.FromTemplate(7)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(new { Name = "Echo" })
            .Build();

        var postmark = new RecordingPostmarkClient(new EmailResponse
        {
            MessageId = Guid.NewGuid()
                .ToString("D"),
            ErrorCode = 0,
            Message = "OK"
        });
        var logger = new TestLogger();
        var client = new PostKitClient(postmark, logger);

        var result = await client.SendEmailAsync(email, CancellationToken.None);

        Assert.True(result.IsFailure(), result.ToString());
        Assert.Contains("SubmittedAt was not returned", result.ToString(), StringComparison.Ordinal);
    }

    private sealed class RecordingPostmarkClient(EmailResponse response) : IPostmarkClient
    {
        public RecordingPostmarkClient() : this(new EmailResponse
        {
            MessageId = Guid.NewGuid()
                .ToString(),
            SubmittedAt = DateTimeOffset.UtcNow,
            ErrorCode = 0,
            Message = ""
        })
        {
        }

        public string? LastEndpoint { get; private set; }

        public object? LastRequest { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastEndpoint = endpoint;
            LastRequest = body;

            if (typeof(TResponse) != typeof(EmailResponse))
                throw new InvalidOperationException("Unexpected response type.");

            return Task.FromResult(Result.Success((TResponse)(object)response));
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("GetAsync should not be called in this test.");
        }
    }

    private sealed class CyclicTemplateModel
    {
        public CyclicTemplateModel? Self { [UsedImplicitly] get; private set; }

        public static CyclicTemplateModel Create()
        {
            var model = new CyclicTemplateModel();
            model.Self = model;
            return model;
        }
    }

    private sealed class TestLogger : ILogger<PostKitClient>
    {
        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
