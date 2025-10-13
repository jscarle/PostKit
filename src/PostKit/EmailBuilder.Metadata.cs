using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private IDictionary<string, string>? _metadata;

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(string name, string value)
    {
        _metadata.EnsureNotSet(nameof(Email.Metadata));

        ValidateMetadataName(name, nameof(name));
        ValidateMetadataValue(value, nameof(value));

        _metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { name, value },
        };

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(KeyValuePair<string, string> entry)
    {
        _metadata.EnsureNotSet(nameof(Email.Metadata));

        ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        _metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { entry.Key, entry.Value },
        };

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _metadata.EnsureNotSet(nameof(Email.Metadata));

        var dictionary = metadata.ToDictionary(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in dictionary)
            ValidateMetadata(entry.Key, entry.Value, nameof(metadata));

        _metadata = dictionary;

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(IDictionary<string, string> metadata)
    {
        _metadata.EnsureNotSet(nameof(Email.Metadata));

        var uniqueKeys = metadata.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != metadata.Keys.Count)
            throw new ArgumentException("There are duplicate metadata entries.", nameof(metadata));

        foreach (var entry in metadata)
            ValidateMetadata(entry.Key, entry.Value, nameof(metadata));

        _metadata = metadata;

        return this;
    }

    private static void ValidateMetadata(ReadOnlySpan<char> name, ReadOnlySpan<char> value, string paramName)
    {
        ValidateMetadataName(name, paramName);
        ValidateMetadataValue(value, paramName);
    }

    private static void ValidateMetadataName(ReadOnlySpan<char> name, string paramName)
    {
        if (!IsValidMetadataName(name))
            throw new ArgumentException("The metadata name is invalid.", paramName);
    }

    private static void ValidateMetadataValue(ReadOnlySpan<char> value, string paramName)
    {
        if (!IsValidMetadataValue(value))
            throw new ArgumentException("The metadata value is invalid.", paramName);
    }

    private static bool IsValidMetadataName(ReadOnlySpan<char> name)
    {
        // Check length constraints
        if (name.Length is 0 or > 20)
            return false;

        // Check if the name starts or ends with whitespace
        if (char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[^1]))
            return false;

        return true;
    }

    private static bool IsValidMetadataValue(ReadOnlySpan<char> value)
    {
        if (value.Length is 0 or > 80)
            return false;

        return true;
    }
}
