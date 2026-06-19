using System.Text.Json;
using LightResults;
using Microsoft.Extensions.Logging;
using PostKit.Postmark;
using PostKit.Postmark.Common;
using PostKit.Templates;
using PostmarkTemplatePushRequest = PostKit.Postmark.Templates.TemplatePushRequest;
using PostmarkTemplateRequest = PostKit.Postmark.Templates.TemplateRequest;
using PostmarkTemplateValidationRequest = PostKit.Postmark.Templates.TemplateValidationRequest;

namespace PostKit.Tests;

public class PostKitClientTemplateResponseTests
{
    [Fact]
    public async Task GetTemplateAsync_WithAlias_MapsTemplate()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/templates/welcome-v1"] = """
                                        {
                                          "TemplateId": 1234,
                                          "Name": "Welcome",
                                          "Subject": "Hello {{name}}",
                                          "HtmlBody": "<strong>Hello</strong>",
                                          "TextBody": "Hello",
                                          "AssociatedServerId": 42,
                                          "Active": true,
                                          "Alias": "welcome-v1",
                                          "TemplateType": "Standard",
                                          "LayoutTemplate": "main-layout"
                                        }
                                        """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.GetTemplateAsync("welcome-v1", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var template), result.ToString());
        Assert.Equal(1234, template.TemplateId);
        Assert.Equal("Welcome", template.Name);
        Assert.Equal("Hello {{name}}", template.Subject);
        Assert.Equal(42, template.AssociatedServerId);
        Assert.Equal(TemplateType.Standard, template.TemplateType);
        Assert.Equal("main-layout", template.LayoutTemplate);
        Assert.Equal("/templates/welcome-v1", postmark.LastGetEndpoint);
    }

    [Fact]
    public async Task ListTemplatesAsync_WithQuery_MapsPage()
    {
        var postmark = new RecordingPostmarkClient(new Dictionary<string, string>
        {
            ["/templates?count=25&offset=50&TemplateType=Layout&LayoutTemplate=main-layout"] = """
                                                                                               {
                                                                                                 "TotalCount": 1,
                                                                                                 "Templates": [
                                                                                                   {
                                                                                                     "TemplateId": 200,
                                                                                                     "Name": "Main Layout",
                                                                                                     "Active": true,
                                                                                                     "Alias": "main-layout",
                                                                                                     "TemplateType": "Layout",
                                                                                                     "LayoutTemplate": null
                                                                                                   }
                                                                                                 ]
                                                                                               }
                                                                                               """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListTemplatesAsync(25, 50, new TemplateQuery { TemplateType = TemplateListType.Layout, LayoutTemplate = "main-layout" }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var page), result.ToString());
        Assert.Equal(1, page.TotalCount);
        var template = Assert.Single(page.Templates);
        Assert.Equal(200, template.TemplateId);
        Assert.Equal(TemplateType.Layout, template.TemplateType);
    }

    [Fact]
    public async Task CreateTemplateAsync_SendsCreateRequest()
    {
        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string>
        {
            ["/templates"] = """
                             {
                               "TemplateId": 1234,
                               "Name": "Welcome",
                               "Active": true,
                               "Alias": "welcome-v1",
                               "TemplateType": "Standard",
                               "LayoutTemplate": "main-layout"
                             }
                             """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateTemplateAsync(new TemplateCreateParameters
        {
            Name = "Welcome",
            Alias = "welcome-v1",
            Subject = "Hello",
            HtmlBody = "<strong>Hello</strong>",
            TemplateType = TemplateType.Standard,
            LayoutTemplate = "main-layout"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var template), result.ToString());
        Assert.Equal(1234, template.TemplateId);
        Assert.Equal("/templates", postmark.LastPostEndpoint);
        var body = Assert.IsType<PostmarkTemplateRequest>(postmark.LastPostBody);
        Assert.Equal("Welcome", body.Name);
        Assert.Equal("welcome-v1", body.Alias);
        Assert.Equal("Standard", body.TemplateType);
        Assert.Equal("main-layout", body.LayoutTemplate);
    }

    [Fact]
    public async Task EditTemplateAsync_WithId_SendsPutRequest()
    {
        var postmark = new RecordingPostmarkClient(putResponses: new Dictionary<string, string>
        {
            ["/templates/1234"] = """
                                  {
                                    "TemplateId": 1234,
                                    "Name": "Welcome Updated",
                                    "Active": true,
                                    "Alias": "welcome-v2",
                                    "TemplateType": "Standard",
                                    "LayoutTemplate": null
                                  }
                                  """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.EditTemplateAsync(1234, new TemplateEditParameters
        {
            Name = "Welcome Updated",
            Alias = "welcome-v2",
            Subject = "Hello",
            TextBody = "Hello"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var template), result.ToString());
        Assert.Equal("Welcome Updated", template.Name);
        Assert.Equal("/templates/1234", postmark.LastPutEndpoint);
        var body = Assert.IsType<PostmarkTemplateRequest>(postmark.LastPutBody);
        Assert.Equal("welcome-v2", body.Alias);
    }

    [Fact]
    public async Task DeleteTemplateAsync_WithAlias_MapsDeletion()
    {
        var postmark = new RecordingPostmarkClient(deleteResponses: new Dictionary<string, string>
        {
            ["/templates/welcome-v1"] = """{"ErrorCode":0,"Message":"Template removed."}"""
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.DeleteTemplateAsync("welcome-v1", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var deletion), result.ToString());
        Assert.Equal("Template removed.", deletion.Message);
        Assert.Equal("/templates/welcome-v1", postmark.LastDeleteEndpoint);
    }

    [Fact]
    public async Task ValidateTemplateAsync_MapsValidationResult()
    {
        var postmark = new RecordingPostmarkClient(postResponses: new Dictionary<string, string>
        {
            ["/templates/validate"] = """
                                      {
                                        "AllContentIsValid": false,
                                        "HtmlBody": {
                                          "ContentIsValid": true,
                                          "ValidationErrors": [],
                                          "RenderedContent": "<strong>Alice</strong>"
                                        },
                                        "TextBody": {
                                          "ContentIsValid": false,
                                          "ValidationErrors": [
                                            { "Message": "Bad syntax.", "Line": 1, "CharacterPosition": 3 }
                                          ],
                                          "RenderedContent": null
                                        },
                                        "Subject": null,
                                        "SuggestedTemplateModel": { "name": "Alice" }
                                      }
                                      """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ValidateTemplateAsync(new TemplateValidationParameters
        {
            HtmlBody = "<strong>{{name}}</strong>",
            TextBody = "{{#broken}}",
            TestRenderModel = new { Name = "Alice" }
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var validation), result.ToString());
        Assert.False(validation.AllContentIsValid);
        Assert.True(validation.HtmlBody!.ContentIsValid);
        Assert.False(validation.TextBody!.ContentIsValid);
        var validationError = Assert.Single(validation.TextBody.ValidationErrors);
        Assert.Equal("Bad syntax.", validationError.Message);
        Assert.Equal(1, validationError.Line);
        Assert.Equal(3, validationError.CharacterPosition);
        var body = Assert.IsType<PostmarkTemplateValidationRequest>(postmark.LastPostBody);
        Assert.Equal("Alice", body.TestRenderModel!["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task PushTemplatesAsync_UsesAccountPut()
    {
        var postmark = new RecordingPostmarkClient(accountPutResponses: new Dictionary<string, string>
        {
            ["/templates/push"] = """
                                  {
                                    "TotalCount": 1,
                                    "Templates": [
                                      {
                                        "Action": "Create",
                                        "TemplateId": 1234,
                                        "Alias": "welcome-v1",
                                        "Name": "Welcome",
                                        "TemplateType": "Standard"
                                      }
                                    ]
                                  }
                                  """
        });
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.PushTemplatesAsync(new TemplatePushParameters { SourceServerId = 1, DestinationServerId = 2, PerformChanges = false }, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess(out var push), result.ToString());
        Assert.Equal(1, push.TotalCount);
        var change = Assert.Single(push.Templates);
        Assert.Equal(TemplatePushAction.Create, change.Action);
        Assert.Equal("/templates/push", postmark.LastAccountPutEndpoint);
        var body = Assert.IsType<PostmarkTemplatePushRequest>(postmark.LastAccountPutBody);
        Assert.Equal(1, body.SourceServerId);
        Assert.Equal(2, body.DestinationServerId);
        Assert.False(body.PerformChanges);
    }

    [Fact]
    public async Task CreateTemplateAsync_WithMissingStandardSubject_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateTemplateAsync(new TemplateCreateParameters
        {
            Name = "Welcome",
            HtmlBody = "<strong>Hello</strong>"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The template create parameters subject is required for standard templates.", error.Message);
        Assert.Null(postmark.LastPostEndpoint);
    }

    [Fact]
    public async Task CreateTemplateAsync_WithLayoutTemplateOnLayoutTemplate_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.CreateTemplateAsync(new TemplateCreateParameters
        {
            Name = "Main Layout",
            HtmlBody = "<html>{{{@content}}}</html>",
            TemplateType = TemplateType.Layout,
            LayoutTemplate = "main-layout"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The template create parameters layout template cannot be set for a layout template. Set LayoutTemplate to null for layout templates.", error.Message);
        Assert.Null(postmark.LastPostEndpoint);
    }

    [Fact]
    public async Task ValidateTemplateAsync_WithLayoutTemplateOnLayoutTemplate_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ValidateTemplateAsync(new TemplateValidationParameters
        {
            HtmlBody = "<html>{{{@content}}}</html>",
            TemplateType = TemplateType.Layout,
            LayoutTemplate = "main-layout"
        }, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The template validation parameters layout template cannot be set for a layout template. Set LayoutTemplate to null for layout templates.", error.Message);
        Assert.Null(postmark.LastPostEndpoint);
    }

    [Fact]
    public async Task ListTemplatesAsync_WithWhitespaceLayoutFilter_ReturnsValidationFailureBeforePostmarkCall()
    {
        var postmark = new RecordingPostmarkClient();
        var client = new PostKitClient(postmark, new TestLogger());

        var result = await client.ListTemplatesAsync(query: new TemplateQuery { LayoutTemplate = " " }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure(out var error, out _), result.ToString());
        Assert.Equal("The template list layout template filter cannot be empty or whitespace. Set the value to null to omit it. Actual length: 1.", error.Message);
        Assert.Null(postmark.LastGetEndpoint);
    }

    private sealed class RecordingPostmarkClient(
        Dictionary<string, string>? getResponses = null,
        Dictionary<string, string>? postResponses = null,
        Dictionary<string, string>? putResponses = null,
        Dictionary<string, string>? deleteResponses = null,
        Dictionary<string, string>? accountPutResponses = null) : IPostmarkClient
    {
        public string? LastGetEndpoint { get; private set; }

        public string? LastPostEndpoint { get; private set; }

        public object? LastPostBody { get; private set; }

        public string? LastPutEndpoint { get; private set; }

        public object? LastPutBody { get; private set; }

        public string? LastDeleteEndpoint { get; private set; }

        public string? LastAccountPutEndpoint { get; private set; }

        public object? LastAccountPutBody { get; private set; }

        public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            LastPostEndpoint = endpoint;
            LastPostBody = body;
            return GetResponse<TResponse>(postResponses, endpoint);
        }

        public Task<Result<TResponse>> GetAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastGetEndpoint = endpoint;
            return GetResponse<TResponse>(getResponses, endpoint);
        }

        public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(PostmarkTokenScope tokenScope, string endpoint, TRequest body, CancellationToken cancellationToken = default)
        {
            if (tokenScope == PostmarkTokenScope.Account)
            {
                LastAccountPutEndpoint = endpoint;
                LastAccountPutBody = body;
                return GetResponse<TResponse>(accountPutResponses, endpoint);
            }

            LastPutEndpoint = endpoint;
            LastPutBody = body;
            return GetResponse<TResponse>(putResponses, endpoint);
        }

        public Task<Result<TResponse>> DeleteAsync<TResponse>(PostmarkTokenScope tokenScope, string endpoint, CancellationToken cancellationToken = default)
        {
            LastDeleteEndpoint = endpoint;
            return GetResponse<TResponse>(deleteResponses, endpoint);
        }

        private static Task<Result<TResponse>> GetResponse<TResponse>(Dictionary<string, string>? responses, string endpoint)
        {
            if (responses is null || !responses.TryGetValue(endpoint, out var responseJson))
                throw new InvalidOperationException($"No response was configured for endpoint '{endpoint}'.");

            var response = JsonSerializer.Deserialize<TResponse>(responseJson, PostmarkConfiguration.JsonSerializerOptions);
            if (response is null)
                throw new InvalidOperationException($"Configured response for endpoint '{endpoint}' deserialized to null.");

            return Task.FromResult(Result.Success(response));
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
