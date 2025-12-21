namespace PostKit.Common;

internal static class ValidationExtensions
{
    public static void EnsureNotSet<T>(this T? field, string propertyName)
    {
        if (field is not null)
            throw new InvalidOperationException($"{propertyName} has already been set.");
    }

    /// <summary>
    /// Gets the character count of the input, where each Unicode scalar value (including emojis and other characters represented by surrogate pairs) is counted as a single character.
    /// </summary>
    /// <param name="input">The input span to count characters in.</param>
    /// <returns>The number of Unicode scalar values in the input.</returns>
    public static int GetCharacterCount(this ReadOnlySpan<char> input)
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
