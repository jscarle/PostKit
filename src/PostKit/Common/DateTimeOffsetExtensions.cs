namespace PostKit.Common;

/// <summary>Provides extension methods for working with <see cref="DateTimeOffset" /> objects.</summary>
internal static class DateTimeOffsetExtensions
{
    /// <summary>Converts a <see cref="DateTimeOffset" /> value to a <see cref="DateTimeOffset" /> representing the same moment in Postmark's US Eastern time zone.</summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset" /> instance to convert.</param>
    /// <returns>A <see cref="DateTimeOffset" /> representing the input date and time in Postmark's US Eastern time zone.</returns>
    /// <exception cref="TimeZoneNotFoundException">Thrown if the Postmark time zone cannot be found on the local system.</exception>
    /// <exception cref="InvalidTimeZoneException">Thrown if the time zone data is corrupt.</exception>
    public static DateTimeOffset ToEasternStandardDateTimeOffset(this DateTimeOffset dateTimeOffset)
    {
        return TimeZoneInfo.ConvertTime(dateTimeOffset, TimeZoneHelper.EasternStandardTime);
    }
}