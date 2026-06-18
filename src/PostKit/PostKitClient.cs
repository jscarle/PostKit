using Microsoft.Extensions.Logging;
using PostKit.Postmark;

namespace PostKit;

internal sealed partial class PostKitClient(IPostmarkClient postmark, ILogger<PostKitClient> logger) : IPostKitClient
{
}
