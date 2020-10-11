using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

public class LanguageManager : SingletonBase<LanguageManager>
{
    public static string curLanguage = "Chinese";

    /// <summary>
    /// 读取数据，反射到代码中
    /// </summary>
    public static void SetCodeLanguage()
	{
        Dictionary<string, Dictionary<string, string>> dic = MasterCodeLanguage.Singleton.GetDic();
        FieldInfo[] fieldInfos = typeof(LanguageSet).GetFields();
        foreach(FieldInfo fieldInfo in fieldInfos)
		{
            string key = fieldInfo.Name;
            if(!dic.ContainsKey(key))
			{
                continue;
			}
            fieldInfo.SetValue(null, dic[key][curLanguage]);
        }
    }
    /// <summary>
    /// 对于prefab，每次load替换语言的时候需要手动去调用
    /// 这里不能用脚本自动化，因为无法确定label初始是否是隐藏，以及代码中如何赋值
    /// </summary>
    /// <param name="obj"></param>
    public static void SetPrefabLanguge(GameObject obj)
    {
        Text[] texts = obj.GetComponentsInChildren<Text>(true);
        foreach (Text text in texts)
        {
            if (IsHaveChinese(text.text))
            {
                string key = "";
                if (MasterPrefabLanguage.Singleton.chinese2KeyDic.TryGetValue(text.text, out key))
                {
                    text.text = MasterPrefabLanguage.Singleton.GetDic()[key][curLanguage];
                }
            }
        }
    }

    /// <summary>
    /// 是否包含中文
    /// 这里不太严谨，但是足够了
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsHaveChinese(string str)
    {
        foreach (char c in str)
        {
            if (c > 127)
            {
                return true;
            }
        }
        return false;
    }

}
