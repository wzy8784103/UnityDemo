using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldNode
{
	private static Transform tileParent;
	public static Transform TileParent
	{
		get
		{
			if(tileParent == null)
			{
				GameObject obj = new GameObject();
				obj.name = "TileParent";
				tileParent = obj.transform;
			}
			return tileParent;
		}
	}

	private static Transform objectParent;
	public static Transform ObjectParent
	{
		get
		{
			if (objectParent == null)
			{
				GameObject obj = new GameObject();
				obj.name = "ObjectParent";
				objectParent = obj.transform;
			}
			return objectParent;
		}
	}
}
