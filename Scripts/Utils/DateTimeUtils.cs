using System;
using System.Globalization;

public enum TimeOfDay
{
    Day = 0,
    Night = 1,
    Morning = 2,
    Evening = 3
}

public static class DateTimeUtils
{

    private static readonly DateTime UnixStartDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    private static CultureInfo _enUS = new CultureInfo("en-US");

    public static string GetTimerText(DateTime time)
    {
        if (time.Hour > 0)
        {
            return time.ToString("HH:mm:ss");
        }
        else
        {
            return time.ToString("mm:ss");
        }
        //else if (time.Second >= 10)
        //{
        //    return time.ToString("ss");
        //}
        //else
        //{
        //    return time.ToString("ss").Replace("0", "");
        //}
    }

    public static int GetCurrentTime()
    {
        return (int)(DateTime.UtcNow - UnixStartDate).TotalSeconds;
    }

    public static DateTime GetCurrentDateTime()
    {
        return DateTime.UtcNow;
    }

    public static long GetCurrentTimeInMs()
    {
        return (long)(DateTime.UtcNow - UnixStartDate).TotalMilliseconds;
    }

    public static double GetCurrentTimeDouble()
    {
        return (DateTime.UtcNow - UnixStartDate).TotalSeconds;
    }

    public static DateTime UnixTimeToDateTime(long unixTime)
    {
        return UnixStartDate.Add(TimeSpan.FromSeconds(unixTime));
    }

    public static DateTime UnixTimeMsToDateTime(long unixTimeMs)
    {
        return UnixStartDate.AddMilliseconds(unixTimeMs);
    }

    public static int DateTimeToUnixTime(DateTime dateTime)
    {
        return (int)(dateTime - UnixStartDate).TotalSeconds;
    }

    public static bool IsTimeOfDay(TimeOfDay timeOfDay)
    {
        var currentDateTime = DateTime.Now;
        int hour = currentDateTime.Hour;
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return hour >= 12 && hour < 18;
            case TimeOfDay.Night:
                return hour < 6;
            case TimeOfDay.Morning:
                return hour >= 6 && hour < 12;
            case TimeOfDay.Evening:
                return hour >= 18;
        }
        return false;
    }

    public static DateTime ParseIntDate(int date)
    {
        return date == 0 ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) : new DateTime(date / 10000, (date / 100) % 100, date % 100);
    }

    public static int ParseDateString(string value)
    {
        const string FORMAT = "yyyy-MM-dd";
        DateTime dt;

        if (DateTime.TryParseExact(value, FORMAT, _enUS, DateTimeStyles.None, out dt))
        {
            return (int)(dt - UnixStartDate).TotalSeconds;
        }
        return 0;
    }
    
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}