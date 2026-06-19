using PostKit;
using PostKit.Emails;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPostKit(builder.Configuration);

var app = builder.Build();

var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<IPostKitClient>();
var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

var defaultSender = configuration.GetValue<string>("PostKit:DefaultSender");
ArgumentNullException.ThrowIfNull(defaultSender);

var testRecipient = configuration.GetValue<string>("PostKit:TestRecipient");
ArgumentNullException.ThrowIfNull(testRecipient);

var email = Email.Compose()
    .From(defaultSender)
    .To(testRecipient)
    .Subject("Development Test Message")
    .TextBody("This is a development test message.")
    .Build();

await client.SendEmailAsync(email);