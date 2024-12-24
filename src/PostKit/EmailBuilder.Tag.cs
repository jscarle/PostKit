using PostKit.Validation;

namespace PostKit;

partial class EmailBuilder
{
    private string? _tag;

    public IEmailBuilder WithTag(string tag)
    {
        _tag.EnsureNotSet(nameof(Email.Tag));

        if (tag.Length > 1000)
            throw new ArgumentException("The tag cannot be longer than 1000 characters.", nameof(tag));

        _tag = tag;

        return this;
    }
}
