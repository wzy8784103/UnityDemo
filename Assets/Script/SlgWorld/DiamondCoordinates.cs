using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///			0,0
///		 0,1   1,0
///	  0,2	1,1	  2,0
///		 1,2   2,1
///		    2,2
/// </summary>
public class DiamondCoordinates
{
	public const float w = 2.8f;
	public const float h = 2.8f;
	public const int maxX = 1000;
	public const int maxY = 1000;
	//地图长度，这里就假设长宽都一样了
	public const float worldWidth = maxX * w;

	private const double rad = Math.PI * 45 / 180;
	private static Matrix4x4 matrixScale = new Matrix4x4(
		new Vector4(1 / (DiamondCoordinates.w / (float)Math.Sqrt(2)), 0, 0, 0),
		new Vector4(0, 1, 0, 0),
		new Vector4(0, 0, 1 / (DiamondCoordinates.w / (float)Math.Sqrt(2)), 0),
		new Vector4(0, 0, 0, 1));
	private static Matrix4x4 matrixRotate = new Matrix4x4(
						new Vector4((float)Math.Cos(rad), 0, (float)Math.Sin(rad), 0),
						new Vector4(0, 1, 0, 0),
						new Vector4(-(float)Math.Sin(rad), 0, (float)Math.Cos(rad), 0),
						new Vector4(0, 0, 0, 1));



	//菱形坐标转世界坐标
	public static Vector3 DiamondToWorld(DiamondVector2 pos)
	{
		return new Vector3((pos.x - pos.y) * (w / 2), 0, -(pos.x + pos.y) * (h / 2));
	}
	//世界坐标转菱形坐标
	public static DiamondVector2 WorldToDiamond(Vector3 worldPos)
	{
		//这里用矩阵做，一个缩放矩阵+一个绕y轴旋转矩阵，实际的值还要减去菱形一半的偏移，因为正常坐标系原点在菱形中心
		Vector4 newPos = matrixRotate * (matrixScale * (worldPos - new Vector3(0, 0, DiamondCoordinates.w / 2)));
		//负数的floor就是向高取整，即-0.1取整为-1
		int x = newPos.x < 0 ? (int)Math.Floor(newPos.x) : (int)newPos.x;
		int y = newPos.z > 0 ? -(int)Math.Ceiling(newPos.z) : -(int)newPos.z;
		return new DiamondVector2(x, y);
	}

	//菱形坐标转AOI index
	public static Vector2 DiamondToAOIIndex(DiamondVector2 pos)
	{
		return AOICoordinates.WorldToAOI(DiamondToWorld(pos));
	}
	/// <summary>
	/// 根据摄像机四个角对应的世界坐标+偏移，遍历所有包括的菱形格子
	/// </summary>
	/// <param name="posLB">摄像机左下角</param>
	/// <param name="posLT">摄像机左上角</param>
	/// <param name="posRT">摄像机右上角</param>
	/// <param name="extraDis">偏移，因为如果卡死视野范围，周围过度会不平滑，而且会有露馅</param>
	/// <param name="action">事件回调</param>
	public static void ForeachByWorldPos(Vector3 posLB, Vector3 posLT, Vector3 posRT, float extraDis, Action<DiamondVector2> action)
	{
		//从左上角遍历到右下角，由于菱形地图奇数位会有错位的情况，所以这里加了一个Offset去处理偏移
		float offset = 0;
		float halfWeight = DiamondCoordinates.w / 2;
		float halfHeight = DiamondCoordinates.h / 2;
		for (float y = posLT.z + extraDis; y >= posLB.z - extraDis; y -= halfHeight)
		{
			for (float x = posLT.x - extraDis; x <= posRT.x + extraDis; x += DiamondCoordinates.w)
			{
				DiamondVector2 dPos = DiamondCoordinates.WorldToDiamond(new Vector3(x + offset, 0, y));
				//无效的点不作处理
				if (!DiamondCoordinates.IsValid(dPos))
				{
					continue;
				}
				//Debug.Log("worldpos==" + new Vector3(x + offset, 0, y));
				action(dPos);
			}
			offset = offset == 0 ? halfWeight : 0;
		}
	}

	public static bool IsSameRow(DiamondVector2 a, DiamondVector2 b)
	{
		return (a.x + a.y) == (b.x + b.y);
	}
	public static bool IsSameCol(DiamondVector2 a, DiamondVector2 b)
	{
		return (a.x - a.y) == (b.x - b.y);
	}

	public static bool IsValid(DiamondVector2 pos)
	{
		return pos.x >= 0 && pos.y >= 0 && pos.x < maxX && pos.y < maxY;
	}
}
