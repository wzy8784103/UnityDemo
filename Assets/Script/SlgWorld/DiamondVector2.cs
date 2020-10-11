using UnityEngine;
public struct DiamondVector2
{
	public int x, y;
    public DiamondVector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public static DiamondVector2 operator +(DiamondVector2 a, DiamondVector2 b)
    {
        return new DiamondVector2(a.x + b.x, a.y + b.y);
    }
    public static DiamondVector2 operator -(DiamondVector2 a, DiamondVector2 b)
    {
        return new DiamondVector2(a.x - b.x, a.y - b.y);
    }
    public static bool operator ==(DiamondVector2 a, DiamondVector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(DiamondVector2 a, DiamondVector2 b)
    {
        return a.x != b.x || a.y != b.y;
    }

    //不加了 容易误解
    //public static bool operator <=(DiamondVector2 a, DiamondVector2 b)
    //{
    //	return a.x <= b.x;
    //}
    //   public static bool operator >=(DiamondVector2 a, DiamondVector2 b)
    //   {
    //       return a.x >= b.x;
    //   }
    //public static bool operator >(DiamondVector2 a, int b)
    //{
    //    return a.x > b && a.y > b;
    //}
    //public static bool operator <(DiamondVector2 a, int b)
    //{
    //    return a.x < b || a.y < b;
    //}
    public override bool Equals(object other)
    {
        if (!(other is DiamondVector2))
		{
            return false;
        }
        DiamondVector2 vector = (DiamondVector2)other;
        return x == vector.x && y == vector.y;
    }
    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }
    public override int GetHashCode()
    {
        return GetIndex();
    }

    public bool IsSameRow(DiamondVector2 other)
    {
        return (x + y) == (other.x + other.y);
    }
    public bool IsSameCol(DiamondVector2 other)
    {
        return (x - y) == (other.x - other.y);
    }

    //所有数据存在多个byte数组中，通过线性变换取得索引
    public int GetIndex()
    {
        return y * DiamondCoordinates.maxX + x;
    }
    public static DiamondVector2 GetDiamondPosByIndex(int index)
    {
        return new DiamondVector2(index % DiamondCoordinates.maxX, index / DiamondCoordinates.maxX);
    }
}
