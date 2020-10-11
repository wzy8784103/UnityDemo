using System;
public static class TimeTools
{
    private static long deltaTime = 0;
    /// <summary>
    /// 根据服务器毫秒数计算服务器和本地差值
    /// </summary>
    /// <param name="serverTime">服务器毫秒数</param>
    public static void SetServerDeltaTime(long serverTime)
    {
        deltaTime = serverTime - GetCurLocalMilliseconds();
    }
    private static long GetCurLocalMilliseconds()
    {
        return (long)DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds;
    }
    /// <summary>
    /// 获取校验过的时间，即服务器时间
    /// </summary>
    /// <returns></returns>
    public static long GetCurMilliseconds()
    {
        return GetCurLocalMilliseconds() + deltaTime;
    }
    /// <summary>
    /// 毫秒数转DataTime
    /// </summary>
    /// <param name="serverTime"></param>
    /// <returns></returns>
    public static DateTime ConvertMiliSecondsToDateTime(long serverTime)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return dateTime.Add(new TimeSpan(serverTime * TimeSpan.TicksPerMillisecond));
    }

    /// <summary>
    /// DataTime转毫秒数
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static long ConvertDateTimeToMiliSeconds(DateTime time)
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(time - startTime).TotalMilliseconds;
    }
}
