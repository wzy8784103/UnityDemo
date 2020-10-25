using UnityEngine;
using UnityEditor;

/// <summary>
/// 打包UI和逻辑，都写到这了
/// 基本流程：先列出所有需要代码加载的资源，可以指定文件夹或者代码写，目前用的是代码写
/// 放入singlePathSet中，代码会自动以最优粒度处理依赖并打包
/// </summary>
public class AssetBundleEditorWindow : EditorWindow
{
	//0打包 1热更
	private int selectTab;

	//是否压缩
	private bool isCompress = false;
	//是否全部压缩，否的话差异压缩
	private bool isCompressAll = true;
	//是否更新最新hash
	private bool isUpdateHash = true;
	//是否上传最新压缩包
	private bool isUpLoadHotfixZip = true;
	//是否修改线上版本号
	private bool isChangeVersion = true;

	//是否压缩更新包
	private bool isCompressHotfix = true;
	//是否上传最新hash
	private bool isUpLoadHash = false;


	private void OnGUI()
	{
		selectTab = GUILayout.Toolbar(selectTab, new string[] { "Pack", "Hotfix(需配置7z环境变量)" });
		EditorGUILayout.Space();
		//Pack
		if (selectTab == 0)
		{
			EditorGUILayout.BeginHorizontal();
			isCompress = EditorGUILayout.ToggleLeft("压缩AssetBundle(需下载7z并配置环境变量)", isCompress);
			EditorGUILayout.EndHorizontal();
			if (isCompress)
			{
				EditorGUILayout.BeginHorizontal();
				isCompressAll = EditorGUILayout.ToggleLeft("全部重新压缩", isCompressAll);
				if (GUILayout.Button("单独执行"))
				{
					AssetBundleEditor.CompressAll();
				}
				isCompressAll = EditorGUILayout.ToggleLeft("差异压缩", !isCompressAll);
				if (GUILayout.Button("单独执行"))
				{
					AssetBundleEditor.CompressDiff();
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		//Hotfix
		else if (selectTab == 1)
		{
			EditorGUILayout.BeginHorizontal();
			isUpdateHash = EditorGUILayout.ToggleLeft("更新线上hash文件", isUpdateHash);
			if (GUILayout.Button("单独执行"))
			{
				AssetBundleEditor.DownLoadHash();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isCompressHotfix = EditorGUILayout.ToggleLeft("比较hash差异并压缩(未勾选更新hash将用本地hash)", isCompressHotfix);
			if (GUILayout.Button("单独执行"))
			{
				AssetBundleEditor.CompressForHotfix(isUpdateHash);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isUpLoadHotfixZip = EditorGUILayout.ToggleLeft("上传更新压缩包", isUpLoadHotfixZip);
			if (GUILayout.Button("单独执行"))
			{
				AssetBundleEditor.UpLoadHotfixZip();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			isChangeVersion = EditorGUILayout.ToggleLeft("更新版本号(最后一位+1)", isChangeVersion);
			if (GUILayout.Button("单独执行"))
			{
				AssetBundleEditor.ChangeVersion();
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		isUpLoadHash = EditorGUILayout.ToggleLeft("上传hash文件", isUpLoadHash);
		if (GUILayout.Button("单独执行"))
		{
			AssetBundleEditor.UpLoadNewHash();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		if (GUILayout.Button("开始执行"))
		{
			AssetBundleEditor.BuildAssetBundle();
			if (selectTab == 0)
			{
				if (isCompress)
				{
					if (isCompressAll)
					{
						AssetBundleEditor.CompressAll();
					}
					else
					{
						AssetBundleEditor.CompressDiff();
					}
				}
			}
			else if (selectTab == 1)
			{
				if (isUpdateHash) AssetBundleEditor.DownLoadHash();
				if (isCompressHotfix) AssetBundleEditor.CompressForHotfix(isUpdateHash);
				if (isUpLoadHotfixZip) AssetBundleEditor.UpLoadHotfixZip();
				if (isChangeVersion) AssetBundleEditor.ChangeVersion();
			}
			if (isUpLoadHash)
			{
				AssetBundleEditor.UpLoadNewHash();
			}
		}
	}
}
