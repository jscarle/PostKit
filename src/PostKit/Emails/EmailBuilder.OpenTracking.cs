using PostKit.Postmark.Common;

namespace PostKit.Emails;

partial class EmailBuilder
{
    private bool? _openTracking;

    /// <inheritdoc/>
    public IEmailBuilder WithOpenTracking(bool openTracking = true)
    {
        _openTracking.EnsureNotSet(nameof(Email.OpenTracking));

        _openTracking = openTracking;

        return this;
    }
}
