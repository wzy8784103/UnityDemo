using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.IO;
using System.Text;
using UnityEngine;

public class EditorTools
{
    [MenuItem("Tools/LanguageWindow")]
    public static void OpenLanguageWindow()
    {
        EditorWindow.GetWindow(typeof(LanguageEditorWindow)).Show();
    }

    [MenuItem("Tools/AssetBundleWindow")]
    public static void OpenAssetBundleWindow()
    {
        EditorWindow.GetWindow(typeof(AssetBundleEditorWindow)).Show();
    }

    [MenuItem("Tools/Test")]
    public static void Test()
    {

    }

    public static void RunCommand(string fileName, string command, string workingDirectory, bool isOpenWindow = false)
    {
        Debug.Log(command);
        ProcessStartInfo info = new ProcessStartInfo(fileName);
        //info.Arguments = @"cd C:/Zgame2/trunk/ProtoType5_pack/Assets/StreamingAssets/ & 7z a -t7z 222.7z -aoa Android\assets\resources\mapres\*.assetbundle -r";
        info.Arguments = command;
        info.CreateNoWindow = !isOpenWindow;
        info.ErrorDialog = true;
        info.UseShellExecute = isOpenWindow;
        info.WorkingDirectory = workingDirectory;

        if(!info.UseShellExecute)
		{
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.StandardOutputEncoding = UTF8Encoding.UTF8;
			info.StandardErrorEncoding = UTF8Encoding.UTF8;
		}

        Process process = System.Diagnostics.Process.Start(info);
        if (!info.UseShellExecute)
		{
            using (StreamReader reader = process.StandardError)
            {
                string curLine = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    if (!string.IsNullOrEmpty(curLine))
                    {
                        Debug.Log(curLine);
                    }
                    curLine = reader.ReadLine();
                }
            }
        }
        process.WaitForExit();
        process.Close();
    }

    [MenuItem("Tools/ChangeAllModelShadow")]
    public static void ChangeAllModelShadow()
    {
		string[] stringsPrefab = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Resources/30kAnimatedCharacters" });
		for (int i = 0, count = stringsPrefab.Length; i < count; i++)
		{
			string name = AssetDatabase.GUIDToAssetPath(stringsPrefab[i]);
			//Debug.Log(name);
			GameObject obj = (GameObject)AssetDatabase.LoadMainAssetAtPath(name);
			SkinnedMeshRenderer[] skinRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			foreach (var item in skinRenderers)
			{
				item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				item.receiveShadows = false;
			}

            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var item in meshRenderers)
            {
                item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                item.receiveShadows = false;
            }
        }

		//GameObject obj = Selection.activeGameObject;
		//SkinnedMeshRenderer[] renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		//foreach (var item in renderers)
		//{
		//    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		//    item.receiveShadows = false;
		//}
		Debug.Log("修改完成");
    }
}

