using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailMessageBuilder
{
    private IDictionary<string, string>? _headers;

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithHeader(string name, string value)
    {
        ValidateHeaderName(name, nameof(name));
        ValidateHeaderValue(value, nameof(value));

        if (_headers is null)
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { name, value } };
        else
            _headers.Add(name, value);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithHeader(KeyValuePair<string, string> header)
    {
        ValidateHeader(header.Key, header.Value, nameof(header));

        if (_headers is null)
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { header.Key, header.Value } };
        else
            _headers.Add(header.Key, header.Value);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _headers.EnsureNotSet(nameof(BulkEmailMessage.Headers));

        var headerList = headers.ToList();
        var uniqueKeys = headerList.Select(static header => header.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headerList.Count)
            throw new ArgumentException("There are duplicate header entries.", nameof(headers));

        var dictionary = headerList.ToDictionary(static header => header.Key, static header => header.Value, StringComparer.OrdinalIgnoreCase);

        foreach (var header in dictionary)
            ValidateHeader(header.Key, header.Value, nameof(headers));

        _headers = dictionary;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailMessageBuilder WithHeaders(IDictionary<string, string> headers)
    {
        _headers.EnsureNotSet(nameof(BulkEmailMessage.Headers));

        var uniqueKeys = headers.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headers.Keys.Count)
            throw new ArgumentException("There are duplicate header entries.", nameof(headers));

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
        if (name.IsEmpty)
            return false;

        if (name[0] == '-' || name[^1] == '-')
            return false;

        foreach (var c in name)
        {
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
        var length = value.Length;
        for (var i = 0; i < length; i++)
        {
            var c = value[i];

            if (c is '\r' or '\n')
            {
                if (i == length - 1)
                    return false;

                var next = value[i + 1];
                if (next != ' ' && next != '\t')
                    return false;

                i++;
            }
            else if (c < 0x20 || c > 0x7E)
            {
                return false;
            }
        }

        return true;
    }
}
