/// <summary>
/// 三位版本号 1.2.3
/// 1为给玩家看的版本，比如测试服是0，第一次正式上线为1，下一次大版本更新设为2
/// 2为实际判断是否更新安装包，每次+1
/// 3为热更标记位，每次+1
/// </summary>
public static class VersionTools
{
    public static bool IsUpdateApk(string localVersion, string serverVersion)
    {
        return (GetIntByIndex(serverVersion, 0) > GetIntByIndex(localVersion, 0))
            || (GetIntByIndex(serverVersion, 1) > GetIntByIndex(localVersion, 1));
    }

    public static bool IsSameApk(string localVersion, string serverVersion)
    {
        return (GetIntByIndex(localVersion, 0) == GetIntByIndex(serverVersion, 0)) 
            && (GetIntByIndex(localVersion, 1) == GetIntByIndex(serverVersion, 1));
    }

    public static bool IsUpdateAb(string localVersion, string serverVersion)
    {
        if(IsSameApk(localVersion, serverVersion))
        {
            return GetIntByIndex(localVersion, 2) < GetIntByIndex(serverVersion, 2);
        }
        return false;
    }

    public static string AddApkVersion(string version)
    {
        return GetAddOneInIndex(version, 1);
    }

    public static string AddAbVersion(string version)
    {
        return GetAddOneInIndex(version, 2);
    }

    public static int GetIntByIndex(string version, int index)
    {
        return int.Parse(version.Split('.')[index]);
    }

    private static string GetAddOneInIndex(string version, int index)
    {
        string result = "";
        string[] local = version.Split('.');
        for(int i = 0; i < local.Length; i++)
        {
            if(i == index)
            {
                result += (int.Parse(local[i]) + 1);
            }
            else
            {
                result += local[i];
            }
            result += ".";
        }
        //最后一个点删掉
        return result.Substring(0, result.Length - 1);
    }
}
