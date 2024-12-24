namespace PostKit.Validation;

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
        foreach (var c in input)
        {
            if (char.IsHighSurrogate(c))
                length += 2;
            else
                length += 1;
        }

        return length;
    }
}
