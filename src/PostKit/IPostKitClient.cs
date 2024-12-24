namespace PostKit;

public interface IPostKitClient
{
    Task SendEmailAsync(Email email, CancellationToken cancellationToken = default);
}
