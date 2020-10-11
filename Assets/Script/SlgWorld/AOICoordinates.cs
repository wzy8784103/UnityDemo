using System;
using UnityEngine;

/// <summary>
/// 0,0	1,0 2,0
/// 0,1 1,1 2,1
/// 0,2 1,2 2,2
/// </summary>
public static class AOICoordinates
{
	//aoi的格子在单坐标轴最大数量，数量越大，aoi格子越小
	public const int aoiMaxCount = 100;
	//一个格子的长度
	public const float aoiCellWidth = DiamondCoordinates.worldWidth / aoiMaxCount;

	/// <summary>
	/// 世界坐标转AOI index
	/// 由于我规定的菱形坐标的最上面的尖是0 0，如果以此为坐标原点的话，index会有负数
	/// 所以这里为了统一方便坐标原点是左上角，AOI的Index是从左上角开始
	/// </summary>
	/// <param name="worldPos"></param>
	/// <returns></returns>
	public static Vector2Int WorldToAOI(Vector3 worldPos)
	{
		//对x z做一个处理，因为原点在左上角
		int x = (int)Math.Ceiling((worldPos.x + DiamondCoordinates.worldWidth / 2) / aoiCellWidth) - 1;
		//z要取负
		int z = (int)Math.Ceiling((-worldPos.z + DiamondCoordinates.w / 2) / aoiCellWidth) - 1;
		return new Vector2Int(x, z);
	}

	//根据aoi中心点算出世界坐标
	public static Vector3 AOIToWorldPos(Vector2 aoiPos)
	{
		return new Vector3(aoiPos.x * aoiCellWidth + aoiCellWidth / 2 - DiamondCoordinates.worldWidth / 2, 0, -(aoiPos.y * aoiCellWidth + aoiCellWidth / 2 - DiamondCoordinates.w / 2));
	}

	public static int AOIToIndex(Vector2Int aoiPos)
	{
		return aoiPos.y * aoiMaxCount + aoiPos.x;
	}

	public static bool IsValid(Vector2Int aoiPos)
	{
		return aoiPos.x >= 0 && aoiPos.y >= 0 && aoiPos.x < aoiMaxCount && aoiPos.y < aoiMaxCount;
	}
}
