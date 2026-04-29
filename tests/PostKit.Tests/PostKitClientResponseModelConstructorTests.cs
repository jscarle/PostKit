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
            typeof(BounceSummary),
            typeof(BounceDump),
            typeof(BounceActivation),
            typeof(BulkEmailJob),
        ];

        foreach (var responseModelType in responseModelTypes)
        {
            Assert.Empty(responseModelType.GetConstructors(BindingFlags.Instance | BindingFlags.Public));
            Assert.Contains(responseModelType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic), static constructor => constructor.IsAssembly);
        }
    }

    [Fact]
    public void BounceModels_DoNotExposeTypeCodeProperty()
    {
        Assert.Null(typeof(Bounce).GetProperty("TypeCode", BindingFlags.Instance | BindingFlags.Public));
        Assert.Null(typeof(BounceDetails).GetProperty("TypeCode", BindingFlags.Instance | BindingFlags.Public));
    }
}
