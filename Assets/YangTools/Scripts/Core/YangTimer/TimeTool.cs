using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

/// <summary>
/// 网络校时和兼容现有存档的日期转换工具
/// </summary>
public static class TimeTool
{
    // 是否使用网络时间
    public static bool useNetworkTime = true;
    // 网络时间更新间隔（秒）
    public static float updateInterval = 300f;// 5分钟
    // 当前网络时间
    public static DateTime CurrentNetworkTime
    {
        get => networkTimeAnchor == default ? default : networkTimeAnchor.AddSeconds(RealtimeSeconds - networkTimeAnchorSeconds);
        private set
        {
            networkTimeAnchor = value;
            networkTimeAnchorSeconds = RealtimeSeconds;
        }
    }
    private static DateTime networkTimeAnchor; //最近一次校时结果
    private static double networkTimeAnchorSeconds; //校时时对应的单调时间
    private static double lastUpdateTime; //最近一次校时尝试结束的单调时间
    private static double RealtimeSeconds => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
    // 是否正在获取时间
    private static bool isFetchingTime;
    /// <summary>
    /// 初始化网络校时
    /// </summary>
    public static void Init()
    {
        // 初始化时先获取一次网络时间
        if (useNetworkTime)
        {
            UpdateNetworkTime();
        }
    }
    /// <summary>
    /// 使用不受游戏暂停影响的时间调度校时
    /// </summary>
    public static void Update()
    {
        if (!useNetworkTime) return;
        float interval = updateInterval; //有效校时间隔
        if (interval <= 0f || float.IsNaN(interval) || float.IsInfinity(interval)) interval = 300f;
        if (RealtimeSeconds - lastUpdateTime >= interval && !isFetchingTime)
        {
            UpdateNetworkTime();
        }
    }
    /// <summary>
    /// 异步更新网络时间 成功和失败均遵守重试间隔
    /// </summary>
    public static async void UpdateNetworkTime()
    {
        if (isFetchingTime) return;
        
        isFetchingTime = true;
        try
        {
            //使用Task.Run 在后台线程获取网络时间
            DateTime networkTime = await Task.Run(() => GetNetworkTime());
            CurrentNetworkTime = networkTime;
            Debug.Log($"成功获取网络时间: {networkTime}");
        }
        catch (Exception e)
        {
            Debug.LogError($"获取网络时间失败: {e.Message}");
            //如果获取失败，使用本地时间
            CurrentNetworkTime = DateTime.Now;
        }
        finally
        {
            lastUpdateTime = RealtimeSeconds;
            isFetchingTime = false;
        }
    }
    //获取网络时间的具体实现
    private static DateTime GetNetworkTime()
    {
        // NTP 服务器地址
        string[] ntpServers = {
            "time.windows.com",
            "pool.ntp.org",
            "time.nist.gov"
        };
        
        // 尝试连接多个服务器，直到成功
        foreach (var server in ntpServers)
        {
            try
            {
                const int ntpPort = 123;
                const int ntpPacketSize = 48;
                var ntpEpochStart = new DateTime(1900, 1, 1);
                
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
                    socket.Connect(server, ntpPort);
                    
                    var ntpData = new byte[ntpPacketSize];
                    ntpData[0] = 0x1B; // LI = 0, VN = 3, Mode = 3
                    
                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    
                    byte offsetTransmitTime = 40;
                    ulong intPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime);
                    ulong fractPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime + 4);
                    
                    intPart = SwapEndianness(intPart);
                    fractPart = SwapEndianness(fractPart);
                    var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                    
                    return ntpEpochStart.AddMilliseconds((long)milliseconds).ToLocalTime();
                }
            }
            catch
            {
                continue;//尝试下一个服务器
            }
        }
        
        throw new Exception("所有 NTP 服务器都无法连接");
    }
    //交换字节序
    private static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                      ((x & 0x0000ff00) << 8) +
                      ((x & 0x00ff0000) >> 8) +
                      ((x & 0xff000000) >> 24));
    }
    
    /// <summary>
    /// 启用网络时间且存在缓存时返回校准时间 否则返回本地时间
    /// </summary>
    public static DateTime GetTime()
    {
        return useNetworkTime && networkTimeAnchor != default ? CurrentNetworkTime : DateTime.Now;
    }
  
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
    /// <remarks>保留本地纪元的历史存档格式 此值不是标准 Unix UTC 时间戳</remarks>
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
