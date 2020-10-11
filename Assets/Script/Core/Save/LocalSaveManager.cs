using UnityEngine;

public static class LocalSaveManager
{
    public static bool IsHaveKey(string key, bool isUserKey = true)
    {
        return PlayerPrefs.HasKey(GetFinalKey(key, isUserKey));
    }
    public static string GetString(string key, string defaultValue = "", bool isUserKey = true)
    {
        return PlayerPrefs.GetString(GetFinalKey(key, isUserKey), "");
    }
    public static int GetInt(string key, int defaultValue = 0, bool isUserKey = true)
    {
        return PlayerPrefs.GetInt(GetFinalKey(key, isUserKey), 0);
    }
    public static float GetFloat(string key, float defaultValue = 0, bool isUserKey = true)
    {
        return PlayerPrefs.GetFloat(GetFinalKey(key, isUserKey), 0);
    }
    public static void Save(string key, string value, bool isUserKey = true)
    {
        PlayerPrefs.SetString(GetFinalKey(key, isUserKey), value);
        PlayerPrefs.Save();
    }
    public static void Save(string key, int value, bool isUserKey = true)
    {
        PlayerPrefs.SetInt(GetFinalKey(key, isUserKey), value);
        PlayerPrefs.Save();
    }
    public static void Save(string key, float value, bool isUserKey = true)
    {
        PlayerPrefs.SetFloat(GetFinalKey(key, isUserKey), value);
        PlayerPrefs.Save();
    }
    private static string GetFinalKey(string key, bool isUserKey)
    {
        return isUserKey ? GetUserSaveKey(key) : key;
    }
    private static string GetUserSaveKey(string key)
    {
        return key + "_" + UserModel.Singleton.userId;
    }
}
