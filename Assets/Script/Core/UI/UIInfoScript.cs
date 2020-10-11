using UnityEngine;

public class UIInfoScript : MonoBehaviour
{
	[Tooltip("窗口等级：One.全屏 Two.弹窗 Nomatter.不管理")]
	public UIShowLevel showLevel = UIShowLevel.Two;
}

public enum UIShowLevel
{
	//一级(全屏)
	One,
	//二级（弹窗）
	Two,
	//不管理
	Nomatter,
}
