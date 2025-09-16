using System;
using System.Globalization;
using UnityEngine;

public static class TimeTool
{
    /// <summary>
    /// 获得当天时间标签
    /// </summary>
    public static string GetTodayTag()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
    
    /// <summary>
    /// 获得当前时间戳(秒)
    /// </summary>
    /// <returns></returns>
    public static long GetNowTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
    
    /// <summary>
    /// 时间转换为时间戳--Local
    /// </summary>
    /// <return>秒</return>
    public static long DataTimeConvertToTimeStampLocal(DateTime dataTime = default)
    {
        if (dataTime == default) dataTime = DateTime.Now;
        TimeSpan ts = dataTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
        long timeStampStr = Convert.ToInt64(ts.TotalSeconds);
        return timeStampStr;
    }
    
    /// <summary>
    /// 时间戳转换为时间--Local
    /// </summary>
    /// <param name="timestamp">秒</param>
    public static DateTime TimestampConvertToDate(long timestamp)
    {
        DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        return dtStart.AddSeconds(timestamp);
    }
    
    /// <summary>
    /// 时间转换为时间戳--UTC
    /// </summary>
    /// <return>秒</return>
    public static long DataTimeConvertToTimeStampUtc(DateTime dataTime = default)
    {
        if (dataTime == default) dataTime = DateTime.UtcNow;
        TimeSpan ts = dataTime - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        long timeStampStr = Convert.ToInt64(ts.TotalSeconds);
        return timeStampStr;
    }
    
    /// <summary>
    /// 时间戳转换为时间--UTC
    /// </summary>
    /// <param name="timestamp">秒</param>
    public static DateTime TimestampConvertToDateUtc(this long timestamp)
    {
        DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);
        return dtStart.AddSeconds(timestamp);
    }
    
    /// <summary>
    /// 当天结束时间(24点)
    /// </summary>
    public static DateTime GetDayLastTime()
    {
        return Convert.ToDateTime(DateTime.UtcNow.ToString("yyyy-MM-dd 23:59:59"));
    }

    /// <summary>
    /// 获得给定时间在所在年里是第几周
    /// </summary>
    public static int GetWeekOfYear(DateTime dateTime)
    {
        return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }
}
