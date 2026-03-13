using System.Reflection;
using PostKit.Bounces;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit.Tests;

public class PostKitClientResponseModelConstructorTests
{
    [Fact]
    public void ResponseModels_DoNotExposePublicInstanceConstructors()
    {
        Type[] responseModelTypes =
        [
            typeof(EmailSubmission),
            typeof(EmailBatchSubmission),
            typeof(BouncePage),
            typeof(Bounce),
            typeof(BounceDetails),
            typeof(DeliveryStats),
            typeof(BounceTypeCount),
            typeof(BounceDump),
            typeof(BounceActivation),
            typeof(BulkEmailJob),
        ];

        foreach (var responseModelType in responseModelTypes)
        {
            Assert.Empty(responseModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
            Assert.Contains(
                responseModelType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic),
                static constructor => constructor.IsAssembly
            );
        }
    }
}
