using System;

namespace Utils
{
    public static class TimeExtensions
    {
        public static string ToTimeFormat(this double timeInSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(timeInSeconds);
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}