using UnityEngine;

public static class PlatformManager
{
    public static bool IsEditor()
    {
        return RuntimePlatform.WindowsEditor == Application.platform || RuntimePlatform.OSXEditor == Application.platform;
    }
    public static bool IsIOS()
    {
        return RuntimePlatform.IPhonePlayer == Application.platform;
    }
    public static bool IsAndroid()
    {
        return RuntimePlatform.Android == Application.platform;
    }
}
