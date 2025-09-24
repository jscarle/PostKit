using LightResults;
using PostKit.Responses;

namespace PostKit;

public interface IPostKitClient
{
    Task<Result<SendEmailResponse>> SendEmailAsync(Email email, CancellationToken cancellationToken = default);
}
