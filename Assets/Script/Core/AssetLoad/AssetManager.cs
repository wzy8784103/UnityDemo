using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AssetManager : SingletonBase<AssetManager>
{
	public static IAssetLoad AssetLoad
	{
		get
		{
            if (GameConfig.useAssetBundle)
            {
                return AssetBundleLoad.Singleton;
            }
            else
            {
                return ResourceLoad.Singleton;
			}
        }
	}

	public static string GetAtlasPath(string atlasName)
	{
		return "Assets/Resources/" + atlasName + ".spriteatlas";
	}
}
