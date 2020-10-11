using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用父类，子类可以代码自动生成，但是一些特殊的如翻译表不想遵循规则就需要自己实现
/// 这么实现主要是可以统一管理并且省去注册
/// </summary>
/// <typeparam name="TChild">子类类型</typeparam>
/// <typeparam name="TDic">子类返回容器类型</typeparam>
public abstract class MasterDataBase<TChild, TDic> : SingletonBase<TChild> where TChild : SingletonBase<TChild>, new()
{
    private TDic dic;

    protected ByteBuffer buffer;
    protected int rows;
    protected int cols;
    protected List<string> nameList;
    protected List<EDataType> typeList;
    public TDic Load()
    {
        buffer = LoadByte<TChild>();
        //读出行列
        rows = buffer.ReadInt();
        cols = buffer.ReadInt();
        //读名称
        nameList = ReadNameList(buffer, cols);
        //读类型
        typeList = ReadTypeList(buffer, cols);
        return Init();
    }
    public virtual TDic Init()
    {
        return dic;
    }

    /// <summary>
    /// 做个虚函数把，防止某些数据类需要额外做一些处理什么的
    /// </summary>
    /// <returns></returns>
    public virtual TDic GetDic()
    {
        if (dic == null)
        {
            dic = Load();
        }
        return dic;
    }

    public void Clear()
    {
        dic = default(TDic);
    }

    private ByteBuffer LoadByte<T>()
    {
        TextAsset textAsset = AssetManager.AssetLoad.LoadClearAsset<TextAsset>("Assets/Resources/Data/" + typeof(T).Name + ".bytes");
        if (textAsset == null)
        {
            Debug.LogError("Data不存在==" + typeof(T).Name);
            return null;
        }
        return new ByteBuffer(textAsset.bytes);
    }

    private List<string> ReadNameList(ByteBuffer buffer, int cols)
    {
        List<string> nameList = new List<string>();
        for (int i = 0; i < cols; i++)
        {
            nameList.Add(buffer.ReadString());
        }
        return nameList;
    }
    private List<EDataType> ReadTypeList(ByteBuffer buffer, int cols)
    {
        List<EDataType> typeList = new List<EDataType>();
        for (int i = 0; i < cols; i++)
        {
            typeList.Add((EDataType)buffer.ReadInt());
        }
        return typeList;
    }
}
