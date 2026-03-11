using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private bool? _openTracking;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithOpenTracking(bool openTracking = true)
    {
        _openTracking.EnsureNotSet(nameof(BulkEmail.OpenTracking));

        _openTracking = openTracking;

        return this;
    }
}
