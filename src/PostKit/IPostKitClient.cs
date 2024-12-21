namespace PostKit;

public interface IPostKitClient
{
    Task SendEmailAsync(Email email, MessageStream messageStream = MessageStream.Transactional, CancellationToken cancellationToken = default);
}
