namespace PostKit;

partial class EmailBuilder
{
    private string? _tag;

    public EmailBuilder WithTag(string tag)
    {
        if (tag.Length > 1000)
            throw new ArgumentException("Tag cannot be longer than 1000 characters.", nameof(tag));

        _tag = tag;
        return this;
    }
}
