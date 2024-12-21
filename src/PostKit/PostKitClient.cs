using LightResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostKit.Errors;
using PostKit.Postmark;
using PostKit.Postmark.Email;

namespace PostKit;

internal sealed class PostKitClient : IPostKitClient
{
    private readonly IPostmarkClient _postmark;
    private readonly PostKitOptions _options;
    private readonly ILogger<PostKitClient> _logger;

    internal PostKitClient(IPostmarkClient postmark, PostKitOptions options, ILogger<PostKitClient> logger)
    {
        _postmark = postmark;
        _options = options;
        _logger = logger;
    }

    public async Task SendEmailAsync(Email email, MessageStream messageStream = MessageStream.Transactional, CancellationToken cancellationToken = default)
    {
        var from = email.From.ToString(true);
        var replyTo = email.ReplyTo is not null ? string.Join(",", email.ReplyTo.Select(x => x.ToString(true))) : null;
        var to = string.Join(",", email.To.Select(x => x.GetAddress(true)));
        var cc = email.Cc is not null ? string.Join(",", email.Cc.Select(x => x.GetAddress(true))) : null;
        var bcc = email.Bcc is not null ? string.Join(",", email.Bcc.Select(x => x.GetAddress(true))) : null;
        
        var request = new EmailRequest
        {
            From = from,
            ReplyTo = replyTo,
            To = to,
            Cc = cc,
            Bcc = bcc,
            Subject = email.Subject,
            HtmlBody = email.HtmlBody,
            TextBody = email.TextBody,
            MessageStream = messageStream == MessageStream.Broadcast ? "broadcast" : "outbound",
        };

        Result<EmailResponse> response;
        try
        {
            response = await _postmark.SendAsync<EmailRequest, EmailResponse>("/email", request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while attempting to send the email.");
            return;
        }

        if (response.IsFailure(out var error, out var emailResponse))
            switch (error)
            {
                case PostmarkError postmarkError:
                    _logger.LogError("Failed to send email. {Message} HttpStatusCode: {HttpStatusCode}. PostmarkError: {PostmarkError}", postmarkError.Message, postmarkError.StatusCode, postmarkError.ErrorCode);
                    return;
                case HttpError httpError:
                    _logger.LogError("Failed to send email. {Message} HttpStatusCode: {HttpStatusCode}", httpError.Message, httpError.StatusCode);
                    return;
                default:
                    _logger.LogError("Failed to send email. {Message}", error.Message);
                    return;
            }

        _logger.LogInformation("Sent email to {To}. {Message} {MessageId} {SubmittedAt}", email.To, emailResponse.Message, emailResponse.MessageId, emailResponse.SubmittedAt);
    }
}

public static class PostmarkServiceExtensions
{
    public static void AddPostKit(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddTransient<IPostmarkClient, PostmarkClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var options = sp.GetRequiredService<IOptions<PostKitOptions>>();
                return new PostmarkClient(httpClient, options.Value);
            }
        );

        services.AddTransient<IPostKitClient, PostKitClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<IPostmarkClient>();
                var options = sp.GetRequiredService<IOptions<PostKitOptions>>();
                var logger = sp.GetRequiredService<ILogger<PostKitClient>>();
                return new PostKitClient(httpClient, options.Value, logger);
            }
        );
    }
}
