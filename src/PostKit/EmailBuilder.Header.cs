using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private IDictionary<string, string>? _headers;

    public IEmailBuilder WithHeader(string name, string value)
    {
        _headers.EnsureNotSet(nameof(Email.Headers));

        ValidateHeaderName(name, nameof(name));
        ValidateHeaderValue(value, nameof(value));

        _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { name, value },
        };

        return this;
    }

    public IEmailBuilder WithHeader(KeyValuePair<string, string> header)
    {
        _headers.EnsureNotSet(nameof(Email.Headers));

        ValidateHeader(header.Key, header.Value, nameof(header));

        _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { header.Key, header.Value },
        };

        return this;
    }

    public IEmailBuilder WithHeader(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _headers.EnsureNotSet(nameof(Email.Headers));

        var dictionary = headers.ToDictionary(StringComparer.OrdinalIgnoreCase);

        foreach (var header in dictionary)
            ValidateHeader(header.Key, header.Value, nameof(headers));

        _headers = dictionary;

        return this;
    }

    public IEmailBuilder WithHeader(IDictionary<string, string> headers)
    {
        _headers.EnsureNotSet(nameof(Email.Headers));

        var uniqueKeys = headers.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headers.Keys.Count)
            throw new ArgumentException("There are duplicate metadata entries.", nameof(headers));

        foreach (var header in headers)
            ValidateHeader(header.Key, header.Value, nameof(headers));

        _headers = headers;

        return this;
    }

    private static void ValidateHeader(ReadOnlySpan<char> name, ReadOnlySpan<char> value, string paramName)
    {
        ValidateHeaderName(name, paramName);
        ValidateHeaderValue(value, paramName);
    }

    private static void ValidateHeaderName(ReadOnlySpan<char> name, string paramName)
    {
        if (!IsValidHeaderName(name))
            throw new ArgumentException("The header name is invalid.", paramName);
    }

    private static void ValidateHeaderValue(ReadOnlySpan<char> value, string paramName)
    {
        if (!IsValidHeaderValue(value))
            throw new ArgumentException("The header value is invalid.", paramName);
    }

    private static bool IsValidHeaderName(ReadOnlySpan<char> name)
    {
        // Rules (simplified from RFC 5322 guidelines):
        //  - Must not be empty.
        //  - Allowed characters: letters (A-Z, a-z), digits (0-9), hyphen (-).
        //  - Cannot start or end with a hyphen.
        //  - No spaces or other symbols.
        if (name.IsEmpty)
            return false;

        // Cannot start or end with '-'
        if (name[0] == '-' || name[^1] == '-')
            return false;

        foreach (var c in name)
        {
            // Allowed: A-Z, a-z, 0-9, '-'
            var isLetter = c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            var isDigit = c is >= '0' and <= '9';
            var isHyphen = c == '-';

            if (!isLetter && !isDigit && !isHyphen)
                return false;
        }

        return true;
    }

    private static bool IsValidHeaderValue(ReadOnlySpan<char> value)
    {
        // Rules (simplified from RFC 5322 guidelines):
        //  - Typically includes ASCII 0x20 (space) through 0x7E (~).
        //  - Allows "folding" via CRLF if followed by space or tab.
        //  - Disallows other control characters.
        //  - No bare CR or LF unless it's part of a valid fold.
        var length = value.Length;
        for (var i = 0; i < length; i++)
        {
            var c = value[i];

            // Check for line folding: CR or LF must be followed by SP or HT
            if (c is '\r' or '\n')
            {
                // Must not be the last character (can't fold at the very end)
                if (i == length - 1)
                    return false;

                // Next character must be space or tab for valid folding
                var next = value[i + 1];
                if (next != ' ' && next != '\t')
                    return false;

                // Skip the next char because we treat CRLF + space/tab as a single folding token
                i++;
            }
            else
            {
                // Check that it's within printable range: 0x20 (space) to 0x7E (~)
                if (c < 0x20 || c > 0x7E)
                    return false;
            }
        }

        return true;
    }
}
