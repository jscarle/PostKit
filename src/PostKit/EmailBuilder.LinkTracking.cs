using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private LinkTracking? _linkTracking;

    /// <inheritdoc />
    public IEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _linkTracking.EnsureNotSet(nameof(Email.LinkTracking));

        _linkTracking = linkTracking;

        return this;
    }
}
