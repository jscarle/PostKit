using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostKit.Postmark.Common;

internal static class ValidationExtensions
{
    public static void EnsureNotSet<T>(this T? field, string propertyName)
    {
        if (field is not null)
            throw new InvalidOperationException($"{propertyName} has already been set.");
    }

    /// <summary>
    /// Gets the length using UTF-16 code units, which matches how Postmark applies the documented limits for fields such as <c>From</c> and <c>Subject</c>.
    /// </summary>
    /// <param name="input">The input span to measure.</param>
    /// <returns>The number of UTF-16 code units.</returns>
    public static int GetPostmarkCharacterCount(this ReadOnlySpan<char> input)
    {
        return input.Length;
    }

    public static bool IsValidMessageStreamId(this ReadOnlySpan<char> streamId)
    {
        if (streamId.Length is 0 or > 30)
            return false;

        if (char.IsDigit(streamId[0]))
            return false;

        if (streamId[0] == '-' || streamId[^1] == '-')
            return false;

        if (streamId.StartsWith("pm-", StringComparison.OrdinalIgnoreCase))
            return false;

        var previousWasDash = false;
        foreach (var ch in streamId)
        {
            var isLowercaseLetter = ch is >= 'a' and <= 'z';
            var isDigit = ch is >= '0' and <= '9';
            var isHyphen = ch == '-';
            var isUnderscore = ch == '_';

            if (!isLowercaseLetter && !isDigit && !isHyphen && !isUnderscore)
                return false;

            if (isHyphen)
            {
                if (previousWasDash)
                    return false;

                previousWasDash = true;
            }
            else
            {
                previousWasDash = false;
            }
        }

        return true;
    }

    public static bool IsValidHeaderName(this ReadOnlySpan<char> name)
    {
        if (name.IsEmpty)
            return false;

        foreach (var c in name)
        {
            var isVisibleAscii = c is >= (char)0x21 and <= (char)0x7E;
            if (!isVisibleAscii || c == ':')
                return false;
        }

        return true;
    }

    public static bool IsValidHeaderValue(this ReadOnlySpan<char> value)
    {
        // Empty values are valid on purpose. A live call on 2026-03-13 showed Postmark accepting a custom
        // header with a missing/null Value, so do not "fix" this without re-validating the live API first.
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (current == '\r')
            {
                if (i + 2 >= value.Length)
                    return false;

                if (value[i + 1] != '\n')
                    return false;

                var foldingWhitespace = value[i + 2];
                if (foldingWhitespace != ' ' && foldingWhitespace != '\t')
                    return false;

                i += 2;
                continue;
            }

            if (current == '\n')
                return false;

            if ((current < 0x20 && current != '\t') || current > 0x7E)
                return false;
        }

        return true;
    }

    public static (JsonNode Snapshot, int SerializedSizeInBytes) SnapshotTemplateModel(this object templateModel, string paramName, JsonSerializerOptions? serializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(templateModel, paramName);

        try
        {
            var effectiveSerializerOptions = PostKitTemplateModelSerialization.Resolve(serializerOptions);
            var snapshot = templateModel switch
            {
                JsonNode jsonNode => jsonNode.DeepClone(),
                _ => JsonSerializer.SerializeToNode(templateModel, effectiveSerializerOptions),
            };

            if (snapshot is null)
                throw new ArgumentException("The template model must serialize to a non-null JSON value.", paramName);

            if (snapshot is not JsonObject)
                throw new ArgumentException("The template model must serialize to a JSON object.", paramName);

            var serializedSize = JsonSizeEstimator.GetSerializedSize(snapshot, effectiveSerializerOptions);
            return (snapshot, serializedSize);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            throw new ArgumentException("The template model could not be serialized.", paramName, ex);
        }
    }
}
