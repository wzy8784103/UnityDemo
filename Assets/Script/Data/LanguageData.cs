using System.Collections.Generic;
using UnityEngine;

public class MasterCodeLanguage : MasterDataBase<MasterCodeLanguage, Dictionary<string, Dictionary<string, string>>>
{
    public override Dictionary<string, Dictionary<string, string>> Init()
    {
        //Dictionary<key, Dictionary<语言名, 对应的翻译>>,这种格式可以动态加语言
        Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
        for (int r = DataManager.startRow; r <= rows; r++)
        {
            string key = buffer.ReadString();
            if (!result.ContainsKey(key))
            {
                result.Add(key, new Dictionary<string, string>());
            }
            for (int i = 1; i < nameList.Count; i++)
            {
                result[key].Add(nameList[i], buffer.ReadString());
            }
        }
        return result;
    }
}

public class MasterPrefabLanguage : MasterDataBase<MasterPrefabLanguage, Dictionary<string, Dictionary<string, string>>>
{
    public Dictionary<string, string> chinese2KeyDic = new Dictionary<string, string>();

    public override Dictionary<string, Dictionary<string, string>> Init()
    {
        chinese2KeyDic.Clear();
        //Dictionary<key, Dictionary<语言名, 对应的翻译>>,这种格式可以动态加语言
        Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
        for (int r = DataManager.startRow; r <= rows; r++)
        {
            string key = buffer.ReadString();
            if (!result.ContainsKey(key))
            {
                result.Add(key, new Dictionary<string, string>());
            }
            for (int i = 1; i < nameList.Count; i++)
            {
                string value = buffer.ReadString();
                result[key].Add(nameList[i], value);
                //第二列为中文
                if(i == 1)
                {
                    if (!chinese2KeyDic.ContainsKey(value))
                    {
                        chinese2KeyDic.Add(value, key);
                    }
                    else
                    {
                        Debug.LogError("MasterPrefabLanguage表中有相同的中文，key：" + key);
                    }
                }
            }
        }
        return result;
    }
}

public class MasterExcelLanguage : MasterDataBase<MasterExcelLanguage, Dictionary<string, Dictionary<string, string>>>
{
    public Dictionary<string, string> chinese2KeyDic = new Dictionary<string, string>();

    public override Dictionary<string, Dictionary<string, string>> Init()
    {
        chinese2KeyDic.Clear();
        //Dictionary<key, Dictionary<语言名, 对应的翻译>>,这种格式可以动态加语言
        Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
        for (int r = DataManager.startRow; r <= rows; r++)
        {
            string key = buffer.ReadString();
            if (!result.ContainsKey(key))
            {
                result.Add(key, new Dictionary<string, string>());
            }
            for (int i = 1; i < nameList.Count; i++)
            {
                string value = buffer.ReadString();
                result[key].Add(nameList[i], value);
                //第二列为中文
                if (i == 1)
                {
                    if (!chinese2KeyDic.ContainsKey(value))
                    {
                        chinese2KeyDic.Add(value, key);
                    }
                    else
                    {
                        Debug.LogError("MasterExcelLanguage表中有相同的中文，key：" + key);
                    }
                }
            }
        }
        return result;
    }
}