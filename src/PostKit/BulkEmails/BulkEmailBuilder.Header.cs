using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private Dictionary<string, string>? _headers;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithHeader(string name, string value)
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
    public IBulkEmailBuilder WithHeader(KeyValuePair<string, string> header)
    {
        ValidateHeader(header.Key, header.Value, nameof(header));

        if (_headers is null)
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { header.Key, header.Value } };
        else
            _headers.Add(header.Key, header.Value);

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder WithHeaders(IEnumerable<KeyValuePair<string, string>> headers)
    {
        _headers.EnsureNotSet(nameof(BulkEmail.Headers));

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
    public IBulkEmailBuilder WithHeaders(IDictionary<string, string> headers)
    {
        _headers.EnsureNotSet(nameof(BulkEmail.Headers));

        var uniqueKeys = headers.Keys
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (uniqueKeys != headers.Keys.Count)
            throw new ArgumentException("There are duplicate header entries.", nameof(headers));

        foreach (var header in headers)
            ValidateHeader(header.Key, header.Value, nameof(headers));

        _headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);

        return this;
    }

    private static void ValidateHeader(ReadOnlySpan<char> name, ReadOnlySpan<char> value, string paramName)
    {
        ValidateHeaderName(name, paramName);
        ValidateHeaderValue(value, paramName);
    }

    private static void ValidateHeaderName(ReadOnlySpan<char> name, string paramName)
    {
        if (!name.IsValidHeaderName())
            throw new ArgumentException("The header name is invalid.", paramName);
    }

    private static void ValidateHeaderValue(ReadOnlySpan<char> value, string paramName)
    {
        if (!value.IsValidHeaderValue())
            throw new ArgumentException("The header value is invalid.", paramName);
    }
}
