using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private bool? _openTracking;

    public IEmailBuilder WithOpenTracking(bool openTracking = true)
    {
        _openTracking.EnsureNotSet(nameof(Email.OpenTracking));

        _openTracking = openTracking;

        return this;
    }
}
