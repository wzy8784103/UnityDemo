using System;
using System.Collections.Generic;
using UnityEngine;
public class UIWorldPopup : UIBase
{
	public override void OnInit()
	{
		DiamondVector2 pos = (DiamondVector2)arg;
		//坐标
		Transform bgTf = baseObject.transform.Find("Canvas/Bg");
		UITools.SetText(bgTf.Find("Title"), LanguageSet.Tip_1);
		UITools.SetText(bgTf.Find("Text"), pos.ToString());
		UITools.BindOnClick(baseObject.transform.Find("Canvas/Image"), OnClickClose);
	}
	
	private void OnClickClose(GameObject obj)
	{
		Close();
	}

	public override void OnDestory()
	{
	}
}
