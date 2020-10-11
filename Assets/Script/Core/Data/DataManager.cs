using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DataManager : SingletonBase<DataManager>
{
    //数据在Excel开始的行数，这个写个变量，怕以后改啥的，很多读写的地方都用到开始值了
    public const int startRow = 4;

    /// <summary>
    /// 通过反射从bytes中读取到类中
    /// </summary>
    /// <typeparam name="T">目标类</typeparam>
    /// <param name="t">目标类实例</param>
    /// <param name="buffer">bytes</param>
    /// <param name="nameList">字段名字列表</param>
    /// <param name="typeList">字段类型列表</param>
    /// <param name="fieldDict">反射缓存容器</param>
    public void ReadReflect<T>(T t, ByteBuffer buffer, List<string> nameList, List<EDataType> typeList, Dictionary<string, FieldInfo> fieldDict)
    {
        for (int c = 0; c < nameList.Count; c++)
        {
            EDataType type = typeList[c];
            string name = nameList[c];
            object value = null;
            switch (type)
            {
                case EDataType.Int:
                    value = buffer.ReadInt();
                    break;
                case EDataType.Long:
                    value = buffer.ReadLong();
                    break;
                case EDataType.String:
                    value = buffer.ReadString();
                    break;
                case EDataType.Float:
                    value = buffer.ReadFloat();
                    break;
            }
            if (fieldDict.ContainsKey(name))
            {
                fieldDict[name].SetValue(t, value);
            }
            else
            {
                fieldDict.Add(name, ReflectTools.SetValue(t, nameList[c], value));
            }
        }
    }

    public void Clear()
    {

    }
}

public enum EDataType
{
    Int,
    Long,
    String,
    Float
}