using PostKit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPostKit();

var app = builder.Build();

var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<IPostKitClient>();
var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

var defaultSender = configuration.GetValue<string>("PostKit:DefaultSender");
ArgumentNullException.ThrowIfNull(defaultSender);

var testRecipient = configuration.GetValue<string>("PostKit:TestRecipient");
ArgumentNullException.ThrowIfNull(testRecipient);

var email = Email.CreateBuilder()
    .From(defaultSender)
    .To(testRecipient)
    .WithSubject("Development Test Message")
    .WithTextBody("This is a development test message.")
    .Build();

await client.SendEmailAsync(email);
