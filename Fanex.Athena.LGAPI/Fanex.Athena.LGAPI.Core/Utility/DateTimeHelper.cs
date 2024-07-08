using System;

namespace Fanex.Athena.LGAPI.Core.Utility
{
    public static class DateTimeHelper
    {
        public const string Gmt8ZoneId = "Taipei Standard Time";

        public static DateTime ToGMT8(DateTime localValue)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Gmt8ZoneId);
            var localTimeInUTC = localValue.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(localTimeInUTC, timeZoneInfo);
        }
    }
}