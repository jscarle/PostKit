using MimeKit;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit.Tests;

public class BuilderSnapshotTests
{
    [Fact]
    public void EmailBuild_WithExternalRecipientList_IsSnapshot()
    {
        var recipients = new List<MailboxAddress> { new(string.Empty, "first@postkit.com") };

        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To(recipients)
            .WithSubject("Snapshot")
            .WithTextBody("Body")
            .Build();

        recipients.Add(new MailboxAddress(string.Empty, "second@postkit.com"));

        Assert.Single(email.To!);
    }

    [Fact]
    public void EmailBuild_WithExternalMetadataDictionary_IsSnapshot()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Campaign"] = "One" };

        var email = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Snapshot")
            .WithTextBody("Body")
            .WithMetadata(metadata)
            .Build();

        metadata["Region"] = "CA";

        var builtMetadata = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(email.Metadata);

        Assert.Single(builtMetadata);
        Assert.False(builtMetadata.ContainsKey("Region"));
    }

    [Fact]
    public void EmailBuild_WithExternalMetadataDictionaryMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Campaign"] = "One" };

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Snapshot")
            .WithTextBody("Body")
            .WithMetadata(metadata);

        metadata["Region"] = "CA";

        var email = builder.Build();
        var builtMetadata = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(email.Metadata);

        Assert.Single(builtMetadata);
        Assert.False(builtMetadata.ContainsKey("Region"));
    }

    [Fact]
    public void EmailBuild_WithExternalHeaderDictionaryMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["X-Campaign"] = "One" };

        var builder = Email.CreateBuilder()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithSubject("Snapshot")
            .WithTextBody("Body")
            .WithHeaders(headers);

        headers["X-Region"] = "CA";

        var email = builder.Build();
        var builtHeaders = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(email.Headers);

        Assert.Single(builtHeaders);
        Assert.False(builtHeaders.ContainsKey("X-Region"));
    }

    [Fact]
    public void BulkEmailMessageBuild_WithExternalRecipientList_IsSnapshot()
    {
        var recipients = new List<MailboxAddress> { new(string.Empty, "first@postkit.com") };

        var message = BulkEmailMessage.CreateBuilder()
            .To(recipients)
            .Build();

        recipients.Add(new MailboxAddress(string.Empty, "second@postkit.com"));

        Assert.Single(message.To!);
    }

    [Fact]
    public void BulkEmailMessageBuild_WithExternalMetadataDictionaryMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Campaign"] = "One" };

        var builder = BulkEmailMessage.CreateBuilder()
            .To("first@postkit.com")
            .WithMetadata(metadata);

        metadata["Region"] = "CA";

        var message = builder.Build();
        var builtMetadata = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(message.Metadata);

        Assert.Single(builtMetadata);
        Assert.False(builtMetadata.ContainsKey("Region"));
    }
}
