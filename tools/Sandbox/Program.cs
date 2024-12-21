using PostKit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PostKitOptions>(builder.Configuration.GetSection("PostKit"));

builder.Services.AddPostKit();

var app = builder.Build();

var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<IPostKitClient>();

var email = Email.CreateBuilder()
    .From("")
    .To("")
    .WithSubject("Development Test Message")
    .WithTextBody("This is a development test message.")
    .Build();

await client.SendEmailAsync(email);
