using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PostKit.Common;

internal static class ValidationExtensions
{
    private const int MessageStreamIdMaxLength = 30;
    private const int MetadataNameMaxLength = 20;
    private const int MetadataValueMaxLength = 80;
    private const string HeaderNameRuleMessage = "The header name is required and must contain only visible ASCII characters except ':'.";
    private const string HeaderValueRuleMessage = "The header value must contain only visible ASCII characters or tab, and folded lines must use CRLF followed by a space or tab.";
    private const string MetadataNameRuleMessage = "The metadata name is required, must not exceed 20 characters, and cannot start or end with whitespace.";

    public const string MessageStreamIdRequirements = "1-30 characters, start with a lowercase letter, contain only lowercase letters, numbers, '-', or '_', cannot contain consecutive hyphens, and cannot be 'all' or start with 'pm-'";

    public static void EnsureNotSet<T>(this T? field, string propertyName, string? attemptedPropertyName = null, string? guidance = null)
    {
        if (field is not null)
        {
            var attempted = string.IsNullOrWhiteSpace(attemptedPropertyName) ? propertyName : attemptedPropertyName;
            var message = string.Equals(attempted, propertyName, StringComparison.Ordinal)
                ? $"Cannot set {propertyName} because it has already been set."
                : $"Cannot set {attempted} because {propertyName} has already been set.";

            if (!string.IsNullOrWhiteSpace(guidance))
                message = $"{message} {guidance}";

            throw new InvalidOperationException(message);
        }
    }

    /// <summary>Gets the length using UTF-16 code units, which matches how Postmark applies the documented limits for fields such as <c>From</c> and <c>Subject</c>.</summary>
    /// <param name="input">The input span to measure.</param>
    /// <returns>The number of UTF-16 code units.</returns>
    public static int GetPostmarkCharacterCount(this ReadOnlySpan<char> input)
    {
        return input.Length;
    }

    public static bool IsValidMessageStreamId(this ReadOnlySpan<char> streamId)
    {
        if (streamId.Length is 0 or > MessageStreamIdMaxLength)
            return false;

        if (streamId[0] is < 'a' or > 'z')
            return false;

        if (streamId[0] == '-' || streamId[^1] == '-')
            return false;

        if (streamId.StartsWith("pm-", StringComparison.OrdinalIgnoreCase))
            return false;

        if (streamId.Equals("all".AsSpan(), StringComparison.OrdinalIgnoreCase))
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

    public static void ValidateMessageStreamId(string? messageStreamId, string paramName)
    {
        if (messageStreamId is null)
            throw new ArgumentNullException(paramName, "The message stream ID cannot be null.");

        if (!messageStreamId.AsSpan()
                .IsValidMessageStreamId())
            throw new ArgumentException(FormatMessageStreamIdValidationMessage(messageStreamId, "The message stream ID"), paramName);
    }

    public static string FormatMessageStreamIdValidationMessage(string messageStreamId, string subject)
    {
        return $"{subject} must be {MessageStreamIdRequirements}. {GetMessageStreamIdValidationDetail(messageStreamId.AsSpan())}";
    }

    public static void EnsureAddressFirst(string? address, string? name, [CallerMemberName] string methodName = "")
    {
        if (LooksLikeEmailAddress(address) || !LooksLikeEmailAddress(name))
            return;

        throw new ArgumentException($"Display name overloads must specify the email address first: use .{methodName}(\"recipient@example.com\", \"Recipient Name\").", nameof(address));
    }

    public static void ValidateHeader(string? name, string? value, string paramName)
    {
        ValidateHeaderName(name, paramName);
        ValidateHeaderValue(value, paramName);
    }

    public static void ValidateHeaderName(string? name, string paramName)
    {
        if (name is null)
            throw new ArgumentNullException(paramName, "The header name cannot be null.");

        if (!IsValidHeaderName(name.AsSpan()))
            throw new ArgumentException(FormatHeaderNameValidationMessage(name), paramName);
    }

    public static void ValidateHeaderValue(string? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, "The header value cannot be null.");

        if (!IsValidHeaderValue(value.AsSpan()))
            throw new ArgumentException(FormatHeaderValueValidationMessage(value), paramName);
    }

    public static List<KeyValuePair<string, string>> SnapshotValidatedHeaders(IEnumerable<KeyValuePair<string, string>> headers, string paramName, IReadOnlyDictionary<string, string>? existingHeaders = null)
    {
        if (headers is null)
            throw new ArgumentNullException(paramName, "The header collection cannot be null.");

        var headerList = headers.ToList();
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < headerList.Count; index++)
        {
            var header = headerList[index];
            if (header.Key is null)
                throw new ArgumentNullException(paramName, $"The header name at index {index} cannot be null.");

            if (!IsValidHeaderName(header.Key.AsSpan()))
                throw new ArgumentException($"The header name at index {index} is invalid. {FormatHeaderNameValidationMessage(header.Key)}", paramName);

            if (header.Value is null)
                throw new ArgumentNullException(paramName, $"The header value at index {index} cannot be null.");

            if (!IsValidHeaderValue(header.Value.AsSpan()))
                throw new ArgumentException($"The header value at index {index} is invalid. {FormatHeaderValueValidationMessage(header.Value)}", paramName);

            if (seen.TryGetValue(header.Key, out var duplicateIndex))
                throw new ArgumentException($"Header names must be unique and are compared case-insensitively. Header name '{header.Key}' at index {index} duplicates index {duplicateIndex}.", paramName);

            if (existingHeaders is not null && TryGetExistingKey(existingHeaders, header.Key, out var existingHeaderName))
                throw new ArgumentException(FormatDuplicateExistingHeaderMessage(header.Key, existingHeaderName, index), paramName);

            seen.Add(header.Key, index);
        }

        return headerList;
    }

    private static bool IsValidHeaderName(ReadOnlySpan<char> name)
    {
        return GetHeaderNameValidationDetail(name) is null;
    }

    private static bool IsValidHeaderValue(ReadOnlySpan<char> value)
    {
        // Empty values are valid on purpose. A live call on 2026-03-13 showed Postmark accepting a custom
        // header with a missing value, so do not reject empty values without re-validating the live API first.
        return GetHeaderValueValidationDetail(value) is null;
    }

    public static string FormatCharacter(char value)
    {
        return value switch
        {
            ' ' => "space",
            '\t' => "tab",
            >= (char)0x21 and <= (char)0x7E => $"'{value}'",
            _ => $"U+{(int)value:X4}",
        };
    }

    private static string FormatHeaderNameValidationMessage(string name)
    {
        var detail = GetHeaderNameValidationDetail(name.AsSpan());
        return detail is null ? HeaderNameRuleMessage : $"{HeaderNameRuleMessage} {detail}";
    }

    private static string FormatHeaderValueValidationMessage(string value)
    {
        var detail = GetHeaderValueValidationDetail(value.AsSpan());
        return detail is null ? HeaderValueRuleMessage : $"{HeaderValueRuleMessage} {detail}";
    }

    private static string GetMessageStreamIdValidationDetail(ReadOnlySpan<char> streamId)
    {
        if (streamId.IsEmpty)
            return "Actual length: 0.";

        if (streamId.Length > MessageStreamIdMaxLength)
            return $"Actual length: {streamId.Length}.";

        if (streamId.Equals("all".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return "'all' is reserved.";

        if (streamId.StartsWith("pm-", StringComparison.OrdinalIgnoreCase))
            return "The prefix 'pm-' is reserved.";

        if (streamId[0] is < 'a' or > 'z')
            return $"First character must be a lowercase letter. Received {FormatCharacter(streamId[0])} at index 0.";

        if (streamId[^1] == '-')
            return "The message stream ID cannot end with '-'.";

        var previousWasDash = false;
        for (var index = 0; index < streamId.Length; index++)
        {
            var current = streamId[index];
            var isLowercaseLetter = current is >= 'a' and <= 'z';
            var isDigit = current is >= '0' and <= '9';
            var isHyphen = current == '-';
            var isUnderscore = current == '_';

            if (!isLowercaseLetter && !isDigit && !isHyphen && !isUnderscore)
                return $"Invalid character {FormatCharacter(current)} at index {index}.";

            if (isHyphen)
            {
                if (previousWasDash)
                    return $"Consecutive hyphen at index {index}.";

                previousWasDash = true;
            }
            else
            {
                previousWasDash = false;
            }
        }

        return "The value is invalid.";
    }

    private static string? GetHeaderNameValidationDetail(ReadOnlySpan<char> name)
    {
        if (name.IsEmpty)
            return "Actual length: 0.";

        for (var index = 0; index < name.Length; index++)
        {
            var current = name[index];
            var isVisibleAscii = current is >= (char)0x21 and <= (char)0x7E;
            if (!isVisibleAscii || current == ':')
                return $"Invalid character {FormatCharacter(current)} at index {index}.";
        }

        return null;
    }

    private static string? GetHeaderValueValidationDetail(ReadOnlySpan<char> value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (current == '\r')
            {
                if (i + 2 >= value.Length)
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                if (value[i + 1] != '\n')
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                var foldingWhitespace = value[i + 2];
                if (foldingWhitespace != ' ' && foldingWhitespace != '\t')
                    return $"Invalid line folding at index {i}; CRLF must be followed by a space or tab.";

                i += 2;
                continue;
            }

            if (current == '\n')
                return $"Invalid character {FormatCharacter(current)} at index {i}; use CRLF followed by a space or tab for folded lines.";

            if ((current < 0x20 && current != '\t') || current > 0x7E)
                return $"Invalid character {FormatCharacter(current)} at index {i}.";
        }

        return null;
    }

    public static void ValidateMetadata(string? name, string? value, string paramName)
    {
        ValidateMetadataName(name, paramName);
        ValidateMetadataValue(value, paramName);
    }

    public static void ValidateMetadataName(string? name, string paramName)
    {
        if (name is null)
            throw new ArgumentNullException(paramName, "The metadata name cannot be null.");

        if (name.Length > MetadataNameMaxLength)
            throw new ArgumentException($"The metadata name must not exceed {MetadataNameMaxLength} characters. Actual length: {name.Length}.", paramName);

        if (!IsValidMetadataName(name.AsSpan()))
            throw new ArgumentException(FormatMetadataNameValidationMessage(name), paramName);
    }

    public static void ValidateMetadataValue(string? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, "The metadata value cannot be null.");

        if (value.Length > MetadataValueMaxLength)
            throw new ArgumentException($"The metadata value must not exceed {MetadataValueMaxLength} characters. Actual length: {value.Length}.", paramName);
    }

    public static List<KeyValuePair<string, string>> SnapshotValidatedMetadata(IEnumerable<KeyValuePair<string, string>> metadata, string paramName, IReadOnlyDictionary<string, string>? existingMetadata = null)
    {
        if (metadata is null)
            throw new ArgumentNullException(paramName, "The metadata collection cannot be null.");

        var metadataList = metadata.ToList();
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < metadataList.Count; index++)
        {
            var entry = metadataList[index];
            if (entry.Key is null)
                throw new ArgumentNullException(paramName, $"The metadata name at index {index} cannot be null.");

            if (entry.Key.Length > MetadataNameMaxLength)
                throw new ArgumentException($"The metadata name at index {index} must not exceed {MetadataNameMaxLength} characters. Actual length: {entry.Key.Length}.", paramName);

            if (!IsValidMetadataName(entry.Key.AsSpan()))
                throw new ArgumentException($"The metadata name at index {index} is invalid. {FormatMetadataNameValidationMessage(entry.Key)}", paramName);

            if (entry.Value is null)
                throw new ArgumentNullException(paramName, $"The metadata value at index {index} cannot be null.");

            if (entry.Value.Length > MetadataValueMaxLength)
                throw new ArgumentException($"The metadata value at index {index} must not exceed {MetadataValueMaxLength} characters. Actual length: {entry.Value.Length}.", paramName);

            if (seen.TryGetValue(entry.Key, out var duplicateIndex))
                throw new ArgumentException($"Metadata names must be unique and are compared case-insensitively. Metadata name '{entry.Key}' at index {index} duplicates index {duplicateIndex}.", paramName);

            if (existingMetadata is not null && TryGetExistingKey(existingMetadata, entry.Key, out var existingMetadataName))
                throw new ArgumentException(FormatDuplicateExistingMetadataMessage(entry.Key, existingMetadataName, index), paramName);

            seen.Add(entry.Key, index);
        }

        var existingCount = existingMetadata?.Count ?? 0;
        var projectedCount = existingCount + metadataList.Count;
        if (projectedCount > 10)
            throw new ArgumentException($"Cannot set more than 10 metadata fields for a message. Adding {metadataList.Count} metadata fields to the existing {existingCount} would produce {projectedCount}.", paramName);

        return metadataList;
    }

    public static bool TryGetExistingKey(IReadOnlyDictionary<string, string> values, string key, out string existingKey)
    {
        foreach (var entry in values)
        {
            if (string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                existingKey = entry.Key;
                return true;
            }
        }

        existingKey = string.Empty;
        return false;
    }

    public static string FormatDuplicateExistingHeaderMessage(string name, string existingName, int? index = null)
    {
        var nameWithIndex = index.HasValue ? $"Header name '{name}' at index {index.Value}" : $"Header name '{name}'";
        return $"Header names must be unique and are compared case-insensitively. {nameWithIndex} duplicates existing header name '{existingName}'.";
    }

    public static string FormatDuplicateExistingMetadataMessage(string name, string existingName, int? index = null)
    {
        var nameWithIndex = index.HasValue ? $"Metadata name '{name}' at index {index.Value}" : $"Metadata name '{name}'";
        return $"Metadata names must be unique and are compared case-insensitively. {nameWithIndex} duplicates existing metadata name '{existingName}'.";
    }

    public static (JsonNode Snapshot, int SerializedSizeInBytes) SnapshotTemplateModel(this object templateModel, string paramName, JsonSerializerOptions? serializerOptions = null)
    {
        if (templateModel is null)
            throw new ArgumentNullException(paramName, "The template model cannot be null.");

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
            throw new ArgumentException("The template model could not be serialized to a JSON object. Ensure it does not contain cycles or members unsupported by System.Text.Json.", paramName, ex);
        }
    }

    private static bool LooksLikeEmailAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var span = value.AsSpan()
            .Trim();
        var atIndex = span.IndexOf('@');
        if (atIndex <= 0 || atIndex >= span.Length - 1)
            return false;

        foreach (var ch in span)
        {
            if (char.IsWhiteSpace(ch))
                return false;
        }

        return true;
    }

    private static bool IsValidMetadataName(ReadOnlySpan<char> name)
    {
        return GetMetadataNameValidationDetail(name) is null;
    }

    private static string FormatMetadataNameValidationMessage(string name)
    {
        var detail = GetMetadataNameValidationDetail(name.AsSpan());
        return detail is null ? MetadataNameRuleMessage : $"{MetadataNameRuleMessage} {detail}";
    }

    private static string? GetMetadataNameValidationDetail(ReadOnlySpan<char> name)
    {
        if (name.IsEmpty)
            return "Actual length: 0.";

        if (name.Length > MetadataNameMaxLength)
            return $"Actual length: {name.Length}.";

        if (char.IsWhiteSpace(name[0]))
            return $"Invalid leading whitespace {FormatCharacter(name[0])} at index 0.";

        if (char.IsWhiteSpace(name[^1]))
            return $"Invalid trailing whitespace {FormatCharacter(name[^1])} at index {name.Length - 1}.";

        return null;
    }
}
