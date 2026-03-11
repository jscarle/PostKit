using PostKit.Postmark.Common;

namespace PostKit.Emails;

partial class EmailBuilder
{
    private IDictionary<string, string>? _metadata;

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(string name, string value)
    {
        ValidateMetadataName(name, nameof(name));
        ValidateMetadataValue(value, nameof(value));

        if (_metadata is null)
            _metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { name, value } };
        else
        {
            if (_metadata.Count >= 10)
                throw new InvalidOperationException("Cannot add more than 10 metadata values.");
            _metadata.Add(name, value);
        }

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(KeyValuePair<string, string> entry)
    {
        ValidateMetadata(entry.Key, entry.Value, nameof(entry));

        if (_metadata is null)
            _metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { entry.Key, entry.Value } };
        else
        {
            if (_metadata.Count >= 10)
                throw new InvalidOperationException("Cannot add more than 10 metadata values.");
            _metadata.Add(entry.Key, entry.Value);
        }

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder WithMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        _metadata.EnsureNotSet(nameof(Email.Metadata));

        var metadataList = metadata.ToList();
        var uniqueKeys = metadataList.Select(m => m.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != metadataList.Count)
            throw new ArgumentException("There are duplicate metadata entries.", nameof(metadata));

        var dictionary = metadataList.ToDictionary(m => m.Key, m => m.Value, StringComparer.OrdinalIgnoreCase);

        if (dictionary.Count > 10)
            throw new ArgumentException("Cannot set more than 10 metadata values.", nameof(metadata));

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

        if (metadata.Count > 10)
            throw new ArgumentException("Cannot set more than 10 metadata values.", nameof(metadata));

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
