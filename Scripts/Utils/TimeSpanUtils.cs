using System;

namespace Defong.Utils
{
    public static class TimeSpanUtils
    {
        public static string TimeSpanToString(this TimeSpan time)
        {
            var days = (int)Math.Floor(time.TotalDays);
            return days > 0
                ? $"{days:0}d {time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}"
                : ((int)Math.Floor(time.TotalHours) > 0 ? $"{time.Hours:0}:{time.Minutes:00}:{time.Seconds:00}" : $"{time.Minutes:00}:{time.Seconds:00}");
        }

    }
}