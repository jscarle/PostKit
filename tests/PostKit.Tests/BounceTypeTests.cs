using PostKit.Bounces;

namespace PostKit.Tests;

public class BounceTypeTests
{
    [Fact]
    public void BounceType_UsesPostmarkTypeCodes()
    {
        Assert.Equal(1, (int)BounceType.HardBounce);
        Assert.Equal(2, (int)BounceType.Transient);
        Assert.Equal(16, (int)BounceType.Unsubscribe);
        Assert.Equal(32, (int)BounceType.Subscribe);
        Assert.Equal(64, (int)BounceType.AutoResponder);
        Assert.Equal(128, (int)BounceType.AddressChange);
        Assert.Equal(256, (int)BounceType.DnsError);
        Assert.Equal(512, (int)BounceType.SpamNotification);
        Assert.Equal(1024, (int)BounceType.OpenRelayTest);
        Assert.Equal(2048, (int)BounceType.Unknown);
        Assert.Equal(4096, (int)BounceType.SoftBounce);
        Assert.Equal(8192, (int)BounceType.VirusNotification);
        Assert.Equal(16384, (int)BounceType.ChallengeVerification);
        Assert.Equal(100000, (int)BounceType.BadEmailAddress);
        Assert.Equal(100001, (int)BounceType.SpamComplaint);
        Assert.Equal(100002, (int)BounceType.ManuallyDeactivated);
        Assert.Equal(100003, (int)BounceType.Unconfirmed);
        Assert.Equal(100006, (int)BounceType.Blocked);
        Assert.Equal(100007, (int)BounceType.SmtpApiError);
        Assert.Equal(100008, (int)BounceType.InboundError);
        Assert.Equal(100009, (int)BounceType.DmarcPolicy);
        Assert.Equal(100010, (int)BounceType.TemplateRenderingFailed);
#pragma warning disable CS0618
        Assert.Equal(16384, (int)BounceType.MailFrontierMatador);
#pragma warning restore CS0618
    }
}
