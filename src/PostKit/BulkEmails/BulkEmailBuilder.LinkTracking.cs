using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private LinkTracking? _linkTracking;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithLinkTracking(LinkTracking linkTracking = LinkTracking.HtmlAndText)
    {
        _linkTracking.EnsureNotSet(nameof(BulkEmail.LinkTracking));

        _linkTracking = linkTracking;

        return this;
    }
}
