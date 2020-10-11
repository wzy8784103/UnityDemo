using UnityEngine;


public class GameNode : SingletonBase<GameNode>
{
	public const string gameStartName = "GameStart";
	private Transform uiParentTf;
	public Transform UIParentTf
	{
		get
		{
			if(uiParentTf == null)
			{
				uiParentTf = GameObject.Find("Canvas/UI").transform;
			}
			return uiParentTf;
		}
	}
}
