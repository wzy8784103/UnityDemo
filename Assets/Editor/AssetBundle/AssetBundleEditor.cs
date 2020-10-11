using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AssetBundleEditor
{
    /// <summary>
    /// 把所有的shader都加到always include中，要不打assetbundle时候每个用到内置shader都会在自己Bundle中复制一份
    /// 或者也可以把内置shader拷贝到项目里，但是没试过
    /// </summary>
    [MenuItem("Tools/AssetBundle/SetAlwaysIncluderShders")]
    static void SetAlwaysIncluderShders()
    {
        List<string> insideShaderList = new List<string>();
        insideShaderList.Add("Particles/Alpha Blended");
        insideShaderList.Add("Particles/Additive");
        insideShaderList.Add("Mobile/Particles/Additive");
        insideShaderList.Add("Legacy Shaders/Diffuse");
        insideShaderList.Add("Mobile/Particles/Alpha Blended");
        //insideShaderList.Add("Standard");
        insideShaderList.Add("Mobile/Diffuse");
        insideShaderList.Add("Legacy Shaders/Transparent/Diffuse");
        insideShaderList.Add("Legacy Shaders/Self-Illumin/Diffuse");
        insideShaderList.Add("Particles/Additive (Soft)");
        insideShaderList.Add("Unlit/Transparent");
        insideShaderList.Add("Legacy Shaders/Transparent/VertexLit");
        insideShaderList.Add("Legacy Shaders/Reflective/Diffuse");
        insideShaderList.Add("Mobile/Unlit (Supports Lightmap)");
        insideShaderList.Add("Legacy Shaders/Specular");
        insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Diffuse");
        insideShaderList.Add("Particles/Alpha Blended Premultiply");
        insideShaderList.Add("Legacy Shaders/Transparent/Bumped Specular");
        insideShaderList.Add("Unlit/Texture");
        insideShaderList.Add("Skybox/6 Sided");
        insideShaderList.Add("Legacy Shaders/Transparent/Parallax Specular");
        insideShaderList.Add("Legacy Shaders/VertexLit");
        insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Diffuse");
        insideShaderList.Add("Legacy Shaders/Transparent/Bumped Diffuse");
        insideShaderList.Add("Mobile/Bumped Diffuse");
        insideShaderList.Add("Legacy Shaders/Bumped Diffuse");
        insideShaderList.Add("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit");
        insideShaderList.Add("Legacy Shaders/Transparent/Specular");
        insideShaderList.Add("Legacy Shaders/Bumped Specular");
        insideShaderList.Add("Mobile/Bumped Specular");
        //Dictionary<string, bool> assetDic = new Dictionary<string, bool>();
        //Dictionary<string, Shader> shaderDic = new Dictionary<string, Shader>();
        //string[] allAssets = AssetDatabase.GetAllAssetPaths();
        //for (int i = 0, count = allAssets.Length; i < count; i++)
        //{
        //    string dpName = allAssets[i];
        //    if (dpName.EndsWith(".mat"))
        //    {
        //        Material mat = (Material)AssetDatabase.LoadMainAssetAtPath(dpName);
        //        Shader shader = mat.shader;
        //        if (shader.name.Equals("GUI/Text Shader") || shader.name.Equals("Hidden/InternalErrorShader"))
        //        {
        //            continue;
        //        }
        //        if (!shaderDic.ContainsKey(shader.name))
        //        {
        //            shaderDic.Add(shader.name, shader);
        //        }
        //    }
        //}

        SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        SerializedProperty it = graphicsSettings.GetIterator();
        SerializedProperty dataPoint;
        while (it.NextVisible(true))
        {
            if (it.name == "m_AlwaysIncludedShaders")
            {
                it.ClearArray();
                for (int i = 0; i < insideShaderList.Count; i++)
                {
                    it.InsertArrayElementAtIndex(i);
                    dataPoint = it.GetArrayElementAtIndex(i);
                    dataPoint.objectReferenceValue = Shader.Find(insideShaderList[i]);
                }

                graphicsSettings.ApplyModifiedProperties();
            }
        }

        Debug.Log("添加完成");
    }
}
