using UnityEngine;
using UnityEditor;
using System.IO;

public class LanguageExcelPanel
{
    private const string excelName = "L_Excel.xlsx";
    private const string sheetName = "MasterExcelLanguage";
    //存档key
    private const string inputSaveKey = "ExcelInputSaveKey";

    public void DrawUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("", LocalSaveManager.GetString(inputSaveKey, "", false));
        if (GUILayout.Button("选择"))
        {
            string tempOutputPath = EditorUtility.OpenFolderPanel("选择导入目录", LocalSaveManager.GetString(inputSaveKey, "", false), "");
            LocalSaveManager.Save(inputSaveKey, tempOutputPath, false);
        }
        if (GUILayout.Button("导入数据"))
        {
            InputExcel();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void InputExcel()
    {
        string path = LocalSaveManager.GetString(inputSaveKey, "", false);
        if (path == "")
        {
            EditorUtility.DisplayDialog("路径不能为空", "路径不能为空", "确定");
            return;
        }
        string filePath = path + "/" + excelName;
        byte[] bytes = ExcelTools.Excel2Bytes(filePath, sheetName);
        File.WriteAllBytes(LanguageEditorWindow.projectBytesPath + "/" + sheetName + ".bytes", bytes);
        Debug.Log("生成完成");
    }
}
