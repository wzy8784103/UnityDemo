using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEngine.UI;
using System;
using System.IO;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class NewBehaviourScript1 : MonoBehaviour, IPointerClickHandler
{

	//Detect if a click occurs
	public void OnPointerClick(PointerEventData pointerEventData)
	{
		//Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
		Debug.Log(" Game Object Right Clicked!");
	}

	// Use this for initialization
	void Start()
	{
		//SpriteAtlas atlas = AssetsLoadAbstract.Singleton.LoadAsset<SpriteAtlas>("Assets/Resources/New Sprite Atlas.spriteatlas");
		//Sprite sprite = atlas.GetSprite("33");
		//gameObject.transform.Find("Image (2)").GetComponent<Image>().sprite = sprite;
		//gameObject.transform.Find("Image (3)").GetComponent<Image>().sprite = sprite;

		//gameObject.GetComponent<RectTransform>().sizeDelta = 

		//GameObject obj = gameObject;
		//Transform tf = transform;

		//long maxCount = 1000000L;
		//DateTime d1 = DateTime.Now;
		//for(long i = 0; i < maxCount; i++)
		//{
		//	GameObject obj2 = tf.gameObject;
		//}

		//DateTime d2 = DateTime.Now;
		//Debug.LogError(d2 - d1);

		//for (long i = 0; i < maxCount; i++)
		//{
		//	Transform obj2 = obj.transform;
		//}

		//DateTime d3 = DateTime.Now;
		//Debug.LogError(d3 - d2);

		//for (long i = 0; i < maxCount; i++)
		//{
			
		//}

		//DateTime d4 = DateTime.Now;
		//Debug.LogError(d4 - d3);
	}

	// Update is called once per frame
	void Update()
	{
		if (isClick)
		{
			Debug.LogError("============一帧结束");
		}
	}

	private bool isClick = false;
	private void OnGUI()
	{
		//if(GUI.Button(new Rect(100, 100, 100, 100), "存档"))
		//{
		//	ByteBuffer buffer = new ByteBuffer();
		//	buffer.WriteInt(1);
		//	buffer.WriteLong(2);
		//	buffer.WriteString("abc");
		//	File.WriteAllBytes(@"E:\Github\UnityFramework\1.txt", buffer.ToArray());
		//}
		//if (GUI.Button(new Rect(200, 100, 100, 100), "读档"))
		//{
		//	//byte[] bytes = File.ReadAllBytes(@"E:\Github\UnityFramework\1.txt");
		//	//ByteBuffer buffer = new ByteBuffer(bytes.Length);
		//	//buffer.WriteBytes(bytes);
		//	//Debug.Log(buffer.PeekInt());
		//	//Debug.Log(buffer.ReadInt());
		//	//Debug.Log(buffer.ReadLong());
		//	//Debug.Log(buffer.ReadString());

		//	Object origin = AssetLoadBase.Singleton.LoadAsset<Object>("Assets/Resources/GameObject.prefab");
		//	GameObject obj = GameObject.Instantiate(origin) as GameObject;
		//	Resources.UnloadAsset(origin);
		//}

		if (GUI.Button(new Rect(100, 100, 100, 100), "加载场景"))
		{
			//long key = TimeManager.Singleton.CreateTimerOfDuration(10000, OnTimeOver, 1);
			//TimeShowManager.Singleton.CreateTimeText(key, GameObject.Find("UI Root/Label").GetComponent<UILabel>(), "aa");
			//TimeShowManager.Singleton.CreateTimeSlider(key, GameObject.Find("UI Root/Sprite").GetComponent<UISlider>(), 10000);
			////isClick = true;
			////StartCoroutine(TestStartCoroutine());
			////AssetLoadBase.Singleton.LoadSceneAsync("Assets/zhucheng_new/Scenes/zhucheng_new_chuntian.unity", null);
			//GameObject temp = AssetManager.LoadBase.LoadAsset<GameObject>("Assets/Resources/Canvas 1.prefab", null);
			//GameObject obj = GameObject.Instantiate(temp);

			UIManager.Singleton.OpenWindow<UIWorldPopup>();
		}
		//if (GUI.Button(new Rect(200, 100, 100, 100), "加载prefab"))
		//{
		//	isClick = true;
		//	Debug.LogError("isClick");
		//	//GameObject temp = AssetManager.LoadBase.LoadAsset<GameObject>("Assets/Resources/GameObject (1).prefab");
		//	//GameObject obj = GameObject.Instantiate(temp);
		//	List<string> pathList = new List<string>();
		//	pathList.Add("Assets/Resources/Test/leijidenglu_01.jpg");
		//	//pathList.Add("Assets/Resources/Test/tex_other_02.png");
		//	//AssetManager.LoadBase.LoadAssetAsync<Texture2D>("Assets/Resources/Test/leijidenglu_01.png", OnLoadOver);
		//	AssetManager.LoadBase.LoadAssetAsync<Texture2D>(pathList, OnLoadOver);
		//	AssetManager.LoadBase.LoadAssetAsync<GameObject>("Assets/Resources/Test/UI Root.prefab", OnLoadOver2);
		//}
		//if (GUI.Button(new Rect(300, 100, 100, 100), "卸载prefab"))
		//{
		//	Debug.Log(TimeShowManager.GetShowTime(0));

		//	//AssetLoadBase.Singleton.UnLoadAsset("Assets/Resources/GameObject (1).prefab");

		//	//FileTools.Copy(AssetBundleLoad.GetAbParentPath() + "123.txt", AssetBundleLoad.GetAbParentPath() + "22.txt");
		//}
	}

	private void OnTimeOver(object arg)
	{
		Debug.LogError("OnTimeOver==" + arg);
	}

	private void OnLoadOver(List<AssetLoadInfo> info)
	{
		foreach(var item in info)
		{
			Debug.LogError("over==" + item.obj);
		}
	}
	private void OnLoadOver2(AssetLoadInfo info)
	{
		GameObject obj = (GameObject)Instantiate(info.obj);
		Debug.LogError("over==" + info.path);
	}
}
