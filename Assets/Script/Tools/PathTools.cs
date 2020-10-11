using UnityEngine;
public static class PathTools
{
    public static string GetStreamingPath()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform  == RuntimePlatform.OSXEditor
            || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return "file://" + Application.streamingAssetsPath + "/";
        }
        return Application.streamingAssetsPath + "/";
    }
}
