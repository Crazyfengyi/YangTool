using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public static class TimeTool
{
    // 是否使用网络时间
    public static bool useNetworkTime = true;
    // 网络时间更新间隔（秒）
    public static float updateInterval = 300f;// 5分钟
    // 当前网络时间
    public static DateTime CurrentNetworkTime { get; private set; }
    // 上次更新时间
    private static float lastUpdateTime;
    // 是否正在获取时间
    private static bool isFetchingTime;
    public static void Init()
    {
        // 初始化时先获取一次网络时间
        if (useNetworkTime)
        {
            UpdateNetworkTime();
        }
    }
    public static void Update()
    {
        if (!useNetworkTime) return;
        //有获取过,本地累加模拟,等待下次网络时间覆盖
        if (CurrentNetworkTime != default)
        {
            CurrentNetworkTime = CurrentNetworkTime.AddSeconds(Time.unscaledDeltaTime);
        }
        //定期更新网络时间
        if (Time.time - lastUpdateTime >= updateInterval && !isFetchingTime)
        {
            UpdateNetworkTime();
        }
    }
    //异步更新网络时间
    public static async void UpdateNetworkTime()
    {
        if (isFetchingTime) return;
        
        isFetchingTime = true;
        try
        {
            //使用Task.Run 在后台线程获取网络时间
            DateTime networkTime = await Task.Run(() => GetNetworkTime());
            CurrentNetworkTime = networkTime;
            lastUpdateTime = Time.time;
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
    
    //获取当前时间(如果启用网络时间则返回网络时间,否则返回本地时间)
    public static DateTime GetTime()
    {
        return CurrentNetworkTime != default ? CurrentNetworkTime : DateTime.Now;
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
