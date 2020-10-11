using UnityEngine;
using System.Collections.Generic;

public class GameStart : MonoBehaviour
{
    public bool useAssetBundle = GameConfig.useAssetBundle;
    public bool debugAssetBundle = GameConfig.debugAssetBundle;
    public string language = LanguageManager.curLanguage;

    void Awake()
    {
        GameConfig.useAssetBundle = useAssetBundle;
        GameConfig.debugAssetBundle = debugAssetBundle;
        LanguageManager.curLanguage = language;
    }
    // Use this for initialization
    void Start()
    {
        //将当前UI节点清空
        Transform uiTf = GameObject.Find("Canvas/UI").transform;
        for(int i = 0; i < uiTf.childCount; i++)
		{
            GameObject.Destroy(uiTf.GetChild(i).gameObject);
		}
        LanguageManager.SetCodeLanguage();
        //加载游戏第一个页面
        WorldCreator.Singleton.Init();
    }

    void Update()
    {
		
	}
}

public class Test
{
    public int a;
    public int b;
}
