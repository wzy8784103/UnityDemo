using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;

public static class UITools
{
    public static Text SetText(Transform tf, string value)
    {
		return SetText(tf.gameObject, value);
	}
    public static Text SetText(GameObject obj, string value)
    {
		Text text = obj.GetComponent<Text>();
		SetText(text, value);
		return text;
	}
	public static void SetText(Text text, string value)
	{
		text.text = value;
	}
	public static Slider SetSlider(Transform tf, float value)
	{
		return SetSlider(tf.gameObject, value);
	}
	public static Slider SetSlider(GameObject obj, float value)
	{
		Slider slider = obj.GetComponent<Slider>();
		SetSlider(slider, value);
		return slider;
	}
	public static void SetSlider(Slider slider, float value)
	{
		slider.value = value;
	}
	
	public static Image SetSprite(Transform tf, string spriteName, string atlasName, AutoReleaseBase autoRelease)
	{
		return SetSprite(tf.gameObject, spriteName, atlasName, autoRelease);
	}
	public static Image SetSprite(GameObject obj, string spriteName, string atlasName, AutoReleaseBase autoRelease)
	{
		Image image = obj.GetComponent<Image>();
		SetSprite(image, spriteName, atlasName, autoRelease);
		return image;
	}
	public static void SetSprite(Image image, string spriteName, string atlasName, AutoReleaseBase autoRelease)
	{
		SpriteAtlas atlas = AssetManager.AssetLoad.LoadAsset<SpriteAtlas>(GetAtlasPath(atlasName), autoRelease);
		image.sprite = atlas.GetSprite(spriteName);
	}
	public static string GetAtlasPath(string atlasName)
	{
		return "Assets/Resources/" + atlasName + ".spriteatlas";
	}
	public static void BindOnClick(GameObject obj, UIEventListener.VoidDelegate action, object arg = null)
	{
		UIEventListener listener = UIEventListener.Get(obj);
		listener.OnClick = (x) => action(obj);
		listener.parameter = arg;
	}
	public static void BindOnClick(Transform tf, UIEventListener.VoidDelegate action, object arg = null)
	{
		BindOnClick(tf.gameObject, action, arg);
	}
	public static bool IsContainsChild(Transform parent, int index)
	{
		if (parent.childCount - 1 > index)
		{
			return true;
		}
		return false;
	}

	public static void CleanGridChild(Transform parent, int count)
	{
		int d = parent.childCount - 1;
		for (int m = count + 1; m <= d; m++)
		{
			GameObject childObject = parent.GetChild(m).gameObject;
			childObject.SetActive(false);
		}
	}

	public static GameObject GetChildByIndex(Transform parent, int index)
	{
		return parent.GetChild(index + 1).gameObject;
	}

	public static GameObject GetGameObject(Transform parentTf, GameObject origin, int index)
	{
		GameObject obj = null;
		if (IsContainsChild(parentTf, index))
		{
			obj = GetChildByIndex(parentTf, index);
		}
		else
		{
			obj = GameObject.Instantiate(origin);
			SetTransform(parentTf, obj);
		}
		obj.SetActive(true);
		return obj;
	}

	public static void SetTransform(Transform parentTf, GameObject obj)
	{
		obj.transform.SetParent(parentTf);
		obj.transform.localEulerAngles = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
	}

	public static T AddMissingComponent<T>(this GameObject obj) where T : Component
	{
		T t = obj.GetComponent<T>();
		if(t == null)
		{
			obj.AddComponent<T>();
		}
		return t;
	}

	public static bool IsRaycastUI()
	{
		if (InputManager.GetMouseButton() || InputManager.GetMouseButtonUp())
		{
			if(PlatformManager.IsEditor())
			{
				return EventSystem.current.IsPointerOverGameObject();
			}
			else
			{
				return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
			}
		}
		return false;
	}
}
