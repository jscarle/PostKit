namespace PostKit.Common;

/// <summary>Provides helper methods and properties for working with time zones within the application.</summary>
internal static class TimeZoneHelper
{
    /// <summary>Gets the <see cref="TimeZoneInfo" /> object representing the US Eastern time zone used by Postmark (America/New_York).</summary>
    /// <remarks>
    ///     On .NET 5 and later, <see cref="TimeZoneInfo.FindSystemTimeZoneById(string)" /> can resolve <a href="https://www.iana.org/time-zones">IANA</a> time
    ///     zone identifiers, even on Windows, when the runtime uses ICU globalization. Windows identifiers also resolve on non-Windows platforms. This mapping isn't
    ///     available when using NLS mode or in globalization invariant mode.
    /// </remarks>
    public static TimeZoneInfo EasternStandardTime { get; } = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
}
