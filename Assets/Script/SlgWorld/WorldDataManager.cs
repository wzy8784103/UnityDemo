using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 数据类，存放地图中所有数据，包含和后台交互
/// </summary>
public class WorldDataManager : SingletonBase<WorldDataManager>
{
    //全地图块的状态 0 无 1激活中
    private byte[] tileStates = new byte[DiamondCoordinates.maxX * DiamondCoordinates.maxY];
    //地图块物体对应的类型 0无 1怪物 2.....
    private byte[] objecTypeBytes = new byte[DiamondCoordinates.maxX * DiamondCoordinates.maxY];
    //地图块物体对应的id
    private byte[] objectIdBytes = new byte[DiamondCoordinates.maxX * DiamondCoordinates.maxY];

    //临时做的地图上怪物id和资源路径的对应关系，正常的话走表，这里就直接写在这了
    private Dictionary<byte, string> resDic = new Dictionary<byte, string>();

    public void SetObjectType(int index, WorldObjectType type)
    {
        objecTypeBytes[index] = (byte)type;
    }
    public WorldObjectType GetObjectType(int index)
    {
        return (WorldObjectType)objecTypeBytes[index];
    }
    public string GetObjectResPath(int index)
	{
        return GetObjectResPath(GetObjectType(index), GetObjectId(index));
    }
    public string GetObjectResPath(WorldObjectType type, byte id)
	{
        string fullPath = "";
        switch (type)
        {
            case WorldObjectType.None:
                break;
            case WorldObjectType.Monster:
                fullPath = resDic[id];
                break;
        }
        return fullPath;
    }
    public void SetObjectId(int index, byte id)
    {
        objectIdBytes[index] = id;
    }
    public byte GetObjectId(int index)
	{
        return objectIdBytes[index];
    }

    public void SetTileState(int index, WorldTileState state)
	{
        tileStates[index] = (byte)state;
    }
    public WorldTileState GetTileState(int index)
    {
        return (WorldTileState)tileStates[index];
    }
    public void Init()
	{
        //临时造一些数据
        resDic.Add(1, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Blacksmiths/NPC_Blacksmith1.prefab");
        resDic.Add(2, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Blacksmiths/NPC_Blacksmith2.prefab");
        resDic.Add(3, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Blacksmiths/NPC_Blacksmith3.prefab");
        resDic.Add(4, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Builders/NPC_Builder1.prefab");
        resDic.Add(5, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Builders/NPC_Builder2.prefab");
        resDic.Add(6, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Builders/NPC_Builder3.prefab");
        resDic.Add(7, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Builders/NPC_Builder4.prefab");
        resDic.Add(8, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Lumberjacks/NPC_Lumberjack1.prefab");
        resDic.Add(9, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Lumberjacks/NPC_Lumberjack2.prefab");
        resDic.Add(10, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Lumberjacks/NPC_Lumberjack3.prefab");
        resDic.Add(11, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Lumberjacks/NPC_Lumberjack4.prefab");
        resDic.Add(12, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Lumberjacks/NPC_Lumberjack5.prefab");
        resDic.Add(13, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Miner/NPC_Miner1.prefab");
        resDic.Add(14, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Miner/NPC_Miner2.prefab");
        resDic.Add(15, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Miner/NPC_Miner3.prefab");
        resDic.Add(16, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/NPC/NPC_Normal1.prefab");
        resDic.Add(17, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/NPC/NPC_Normal2.prefab");
        resDic.Add(18, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/NPC/NPC_Normal3.prefab");
        resDic.Add(19, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/NPC/NPC_Normal4.prefab");
        resDic.Add(20, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Porter/NPC_Porter1.prefab");
        resDic.Add(21, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Porter/NPC_Porter2.prefab");
        resDic.Add(22, "Assets/Resources/30kAnimatedCharacters/Prefabs/Characters/Porter/NPC_Porter3.prefab");

        EventDispatcher.Singleton.AddListener<List<WorldDataDto>>(EventKey.WorldServerDataSend, OnMsgOver, null);
	}

    private void OnMsgOver(List<WorldDataDto> list)
    {
        foreach (var item in list)
        {
            //当前Tile不在激活状态的话，显示层不作处理
            if(GetTileState(item.index) == WorldTileState.None)
			{
                //数据照常同步
                SetObjectType(item.index, item.type);
                SetObjectId(item.index, item.id);
                continue;
			}
            //Debug.Log("item.id==" + item.id + ", item.index==" + item.index + ",objectIdBytes[item.index]==" + objectIds[item.index]);
            //如果当前的id是新增的或者和之前不一样，则走添加逻辑
            //先拿以下旧的数据
            byte oldId = GetObjectId(item.index);
            DiamondVector2 dPos = DiamondVector2.GetDiamondPosByIndex(item.index);
            if (item.id > 0 && (oldId == 0 || oldId != item.id))
            {
                //先把之前的移除
                if (oldId > 0)
				{
                    EventDispatcher.Singleton.NotifyListener(EventKey.WorldObjectRemove, dPos, GetObjectResPath(item.type, oldId));
                }
                EventDispatcher.Singleton.NotifyListener(EventKey.WorldObjectAdd, dPos, GetObjectResPath(item.type, item.id));
            }
            //如果之前的数据中有值但是新的数据没值，则走移除逻辑
            else if (item.id == 0 && oldId > 0)
            {
                EventDispatcher.Singleton.NotifyListener(EventKey.WorldObjectRemove, dPos, GetObjectResPath(item.type, oldId));
            }
            SetObjectType(item.index, item.type);
            SetObjectId(item.index, item.id);
        }
    }

    public void ReleaseAll()
	{
        EventDispatcher.Singleton.RemoveListener<List<WorldDataDto>>(EventKey.WorldServerDataSend, OnMsgOver);

    }
}

public enum WorldTileState
{
    None,
    Active  //激活中
}
public enum WorldObjectType
{
    None,
    Monster,//怪物
}
