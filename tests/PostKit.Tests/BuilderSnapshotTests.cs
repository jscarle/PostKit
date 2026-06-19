using System.Text.Json.Nodes;
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

        var email = Email.Compose()
            .From("sender@postkit.com")
            .To(recipients)
            .Subject("Snapshot")
            .TextBody("Body")
            .Build();

        recipients.Add(new MailboxAddress(string.Empty, "second@postkit.com"));

        Assert.Single(email.To!);
    }

    [Fact]
    public void EmailBuild_WithExternalRecipientListMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        IList<MailboxAddress> recipients = new List<MailboxAddress> { new(string.Empty, "first@postkit.com") };

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To(recipients)
            .Subject("Snapshot")
            .TextBody("Body");

        recipients.Add(new MailboxAddress(string.Empty, "second@postkit.com"));

        var email = builder.Build();

        Assert.Single(email.To!);
    }

    [Fact]
    public void EmailBuild_WithExternalFromMailboxAddressMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var sender = new MailboxAddress("Sender", "sender@postkit.com");

        var builder = Email.Compose()
            .From(sender)
            .To("recipient@postkit.com")
            .Subject("Snapshot")
            .TextBody("Body");

        sender.Name = "Changed Sender";
        sender.Address = "changed-sender@postkit.com";

        var email = builder.Build();

        Assert.Equal("Sender", email.From!.Name);
        Assert.Equal("sender@postkit.com", email.From.Address);
    }

    [Fact]
    public void EmailBuild_WithExternalRecipientMailboxAddressMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var recipient = new MailboxAddress("Recipient", "recipient@postkit.com");

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To(recipient)
            .Subject("Snapshot")
            .TextBody("Body");

        recipient.Name = "Changed Recipient";
        recipient.Address = "changed-recipient@postkit.com";

        var email = builder.Build();
        var to = Assert.Single(email.To!);

        Assert.Equal("Recipient", to.Name);
        Assert.Equal("recipient@postkit.com", to.Address);
    }

    [Fact]
    public void EmailBuilder_AlsoTo_DoesNotMutateCallerOwnedRecipientList()
    {
        IList<MailboxAddress> recipients = new List<MailboxAddress> { new(string.Empty, "first@postkit.com") };

        var builder = Email.Compose()
            .To(recipients);

        builder.To("second@postkit.com");

        Assert.Single(recipients);
    }

    [Fact]
    public void EmailBuild_WithExternalMetadataDictionary_IsSnapshot()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Campaign"] = "One" };

        var email = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Snapshot")
            .TextBody("Body")
            .AddMetadata(metadata)
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

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Snapshot")
            .TextBody("Body")
            .AddMetadata(metadata);

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

        var builder = Email.Compose()
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .Subject("Snapshot")
            .TextBody("Body")
            .AddHeader(headers);

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

        var message = BulkEmailMessage.Compose()
            .To(recipients)
            .Build();

        recipients.Add(new MailboxAddress(string.Empty, "second@postkit.com"));

        Assert.Single(message.To!);
    }

    [Fact]
    public void BulkEmailBuild_WithExternalFromMailboxAddressMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var sender = new MailboxAddress("Sender", "sender@postkit.com");

        var builder = BulkEmail.Compose()
            .From(sender)
            .Subject("Snapshot")
            .TextBody("Body")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build());

        sender.Name = "Changed Sender";
        sender.Address = "changed-sender@postkit.com";

        var email = builder.Build();

        Assert.Equal("Sender", email.From!.Name);
        Assert.Equal("sender@postkit.com", email.From.Address);
    }

    [Fact]
    public void BulkEmailBuild_WithExternalReplyToMailboxAddressMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var replyTo = new MailboxAddress("Reply", "reply@postkit.com");

        var builder = BulkEmail.Compose()
            .From("sender@postkit.com")
            .ReplyTo(replyTo)
            .Subject("Snapshot")
            .TextBody("Body")
            .AddMessage(BulkEmailMessage.Compose()
                .To("recipient@postkit.com")
                .Build());

        replyTo.Name = "Changed Reply";
        replyTo.Address = "changed-reply@postkit.com";

        var email = builder.Build();
        var builtReplyTo = Assert.Single(email.ReplyTo!);

        Assert.Equal("Reply", builtReplyTo.Name);
        Assert.Equal("reply@postkit.com", builtReplyTo.Address);
    }

    [Fact]
    public void BulkEmailMessageBuild_WithExternalRecipientMailboxAddressMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var recipient = new MailboxAddress("Recipient", "recipient@postkit.com");

        var builder = BulkEmailMessage.Compose()
            .To(recipient);

        recipient.Name = "Changed Recipient";
        recipient.Address = "changed-recipient@postkit.com";

        var message = builder.Build();
        var to = Assert.Single(message.To!);

        Assert.Equal("Recipient", to.Name);
        Assert.Equal("recipient@postkit.com", to.Address);
    }

    [Fact]
    public void BulkEmailBuilder_AlsoReplyTo_DoesNotMutateCallerOwnedReplyToList()
    {
        IList<MailboxAddress> replyTo = new List<MailboxAddress> { new(string.Empty, "reply@postkit.com") };

        var builder = BulkEmail.Compose()
            .ReplyTo(replyTo);

        builder.ReplyTo("other-reply@postkit.com");

        Assert.Single(replyTo);
    }

    [Fact]
    public void BulkEmailMessageBuild_WithExternalMetadataDictionaryMutatedBeforeBuild_UsesAssignmentSnapshot()
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Campaign"] = "One" };

        var builder = BulkEmailMessage.Compose()
            .To("first@postkit.com")
            .AddMetadata(metadata);

        metadata["Region"] = "CA";

        var message = builder.Build();
        var builtMetadata = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(message.Metadata);

        Assert.Single(builtMetadata);
        Assert.False(builtMetadata.ContainsKey("Region"));
    }

    [Fact]
    public void EmailBuild_WithExternalTemplateModelMutatedAfterBuild_UsesSnapshot()
    {
        var templateModel = new JsonObject { ["name"] = "Alice" };

        var email = Email.FromTemplate(42)
            .From("sender@postkit.com")
            .To("recipient@postkit.com")
            .WithModel(templateModel)
            .Build();

        templateModel["name"] = "Bob";

        var builtTemplateModel = Assert.IsType<JsonObject>(email.TemplateModel);
        Assert.Equal("Alice", builtTemplateModel["name"]!.GetValue<string>());
    }

    [Fact]
    public void BulkEmailMessageBuild_WithExternalTemplateModelMutatedAfterBuild_UsesSnapshot()
    {
        var templateModel = new JsonObject { ["name"] = "Alice" };

        var message = BulkEmailMessage.FromTemplate()
            .To("recipient@postkit.com")
            .WithModel(templateModel)
            .Build();

        templateModel["name"] = "Bob";

        var builtTemplateModel = Assert.IsType<JsonObject>(message.TemplateModel);
        Assert.Equal("Alice", builtTemplateModel["name"]!.GetValue<string>());
    }
}