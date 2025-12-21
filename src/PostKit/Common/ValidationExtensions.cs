namespace PostKit.Common;

internal static class ValidationExtensions
{
    public static void EnsureNotSet<T>(this T? field, string propertyName)
    {
        if (field is not null)
            throw new InvalidOperationException($"{propertyName} has already been set.");
    }

    public static int GetUtf16Length(this ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            return 0;

        var length = 0;

        for (var i = 0; i < input.Length; i++)
        {
            if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                i++; // skip low surrogate

            length++;
        }

        return length;
    }
}
