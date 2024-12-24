using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder
{
    private LinkTracking? _linkTracking;

    public IEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _linkTracking.EnsureNotSet(nameof(Email.LinkTracking));

        _linkTracking = linkTracking;

        return this;
    }
}
