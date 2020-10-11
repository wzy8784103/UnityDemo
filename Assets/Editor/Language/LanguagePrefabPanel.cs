using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using LitJson;
using UnityEngine.UI;

public class LanguagePrefabPanel
{
    private const string excelName = "L_Prefab.xlsx";
    private const string sheetName = "MasterPrefabLanguage";
    //存档key
    private const string outputSaveKey = "PrefabOutputSaveKey";
    private const string inputSaveKey = "PrefabInputSaveKey";
    private const string folderSaveKey = "PrefabFolderSaveKey";

    private Vector2 scroll;
    private List<string> pathList = new List<string>();
    private string removeName;

    public void Init()
    {
        pathList = JsonMapper.ToObject<List<string>>(LocalSaveManager.GetString(folderSaveKey, "[]", false));
    }

    public void DrawUI()
    {
        EditorGUILayout.Space();

        if (GUILayout.Button("添加需要多语言功能的prefab文件夹"))
        {
            string path = EditorUtility.OpenFolderPanel("选择导出目录", Application.dataPath, "");
            pathList.Add(path);
            LocalSaveManager.Save(folderSaveKey, JsonMapper.ToJson(pathList), false);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        for (int i = 0, count = pathList.Count; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(pathList[i]);
            if (GUILayout.Button("删除"))
            {
                removeName = pathList[i];
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeName != "")
        {
            pathList.Remove(removeName);
            LocalSaveManager.Save(folderSaveKey, JsonMapper.ToJson(pathList), false);
            removeName = "";
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("", LocalSaveManager.GetString(outputSaveKey, "", false));
        if (GUILayout.Button("选择"))
        {
            string tempOutputPath = EditorUtility.OpenFolderPanel("选择导出目录", LocalSaveManager.GetString(outputSaveKey, "", false), "xlsx");
            LocalSaveManager.Save(outputSaveKey, tempOutputPath, false);
        }
        if (GUILayout.Button("导出数据"))
        {
            OutputExcel();
        }
        EditorGUILayout.EndHorizontal();

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

    private void OutputExcel()
    {
        string outputPath = LocalSaveManager.GetString(outputSaveKey, "", false);
        if (outputPath == "")
        {
            EditorUtility.DisplayDialog("路径不能为空", "路径不能为空", "确定");
            return;
        }

        //string[] stringsPrefab = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Resources/prefab", "Assets/Resources/beginAsset" });

        string[] prefabPaths = AssetDatabase.FindAssets("t:prefab", pathList.ToArray());

        bool isChange = false;
        FileInfo fileInfo = new FileInfo(outputPath + "/" + sheetName);
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet sheet = null;
            if (package.Workbook.Worksheets.Count == 0)
            {
                sheet = package.Workbook.Worksheets.Add(sheetName);
                LanguageEditorWindow.InitSheet(sheet);
            }
            else
            {
                sheet = package.Workbook.Worksheets[sheetName];
            }
            int newRow = DataManager.startRow;
            if (sheet.Dimension.Rows > newRow)
            {
                newRow = sheet.Dimension.Rows + 1;
            }
            int cols = sheet.Dimension.Columns;

            Dictionary<string, string> key2ValueDic = new Dictionary<string, string>();
            Dictionary<string, string> value2KeyDic = new Dictionary<string, string>();
            for (int i = DataManager.startRow; i <= sheet.Dimension.Rows; i++)
            {
                if (sheet.Cells[i, 1].Value == null)
                {
                    newRow = i;
                    break;
                }
                string key = sheet.Cells[i, 1].Value.ToString();
                string value = sheet.Cells[i, 2].Value.ToString();
                key2ValueDic.Add(key, value);
                value2KeyDic.Add(value, key);
            }

            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string name = AssetDatabase.GUIDToAssetPath(prefabPaths[i]);
                //一些不想翻译的
                if (name.Contains("UITest"))
                {
                    continue;
                }
                GameObject obj = (GameObject)AssetDatabase.LoadMainAssetAtPath(name);
                int index = 1;
                Text[] texts = obj.GetComponentsInChildren<Text>(true);
                for (int j = 0; j < texts.Length; j++)
                {
                    Text text = texts[j];
                    if (!LanguageManager.IsHaveChinese(text.text))
                    {
                        continue;
                    }
                    string value = text.text;
                    //如果在prefab中，限制了文字范围，那么ngui会在折行处添加\r，所以要去掉
                    value = value.Replace("\r", "");
                    //如果excel中没有对应中文，就新加一条
                    if(!value2KeyDic.ContainsKey(value))
                    {
                        isChange = true;
                        if (newRow > sheet.Dimension.Rows)
                        {
                            sheet.InsertRow(newRow, 1);
                        }
                        sheet.Cells[newRow, 1].Value = GetKey(key2ValueDic, obj.name, index); ;
                        sheet.Cells[newRow, 2].Value = value;
                        for (int k = 1; k <= cols; k++)
                        {
                            sheet.Cells[newRow, k].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        newRow++;
                    }
                }
            }
            package.Save();
        }
        if (!isChange)
        {
            Debug.Log("无新增项");
        }
        Debug.Log("完成");
    }

    private string GetKey(Dictionary<string, string> key2ValueDic, string prefabName, int index)
    {
        string key = prefabName + "_" + index;
        if (key2ValueDic.ContainsKey(key))
        {
            return GetKey(key2ValueDic, prefabName, index + 1);
        }
        else
        {
            return key;
        }
    }

    /// <summary>
    /// 删除无用的行数,做法是先拿到表里所有的value对应的row，然后遍历所有Prefab中文，在dic中存在就直接删掉，剩下的就是没用的
    /// 这里单独写一个方法而不放到导入里的原因是，比如当前这个表已经给外包翻译了，但是代码中删除了一些字段，
    /// 会造成新生成的表和之前的对不上，单列一个方法，没事的时候手动删一下
    /// </summary>
    private void DeleteUnUseRows()
    {
        string path = LocalSaveManager.GetString(inputSaveKey, "", false);
        if (path == "")
        {
            EditorUtility.DisplayDialog("路径不能为空", "", "确定");
            return;
        }
        string[] prefabPaths = AssetDatabase.FindAssets("t:prefab", pathList.ToArray());

        FileInfo fileInfo = new FileInfo(path + "/" + sheetName);
        Dictionary<string, int> value2RowDic = new Dictionary<string, int>();
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet sheet = package.Workbook.Worksheets[sheetName];
            for (int i = DataManager.startRow; i <= sheet.Dimension.Rows; i++)
            {
                if (sheet.Cells[i, 1].Value == null)
                {
                    break;
                }
                string value = sheet.Cells[i, 2].Value.ToString();
                value2RowDic.Add(value, i);
            }
            //遍历所有prefab
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                string name = AssetDatabase.GUIDToAssetPath(prefabPaths[i]);
                //一些不想翻译的
                if (name.Contains("UITest"))
                {
                    continue;
                }
                GameObject obj = (GameObject)AssetDatabase.LoadMainAssetAtPath(name);
                Text[] texts = obj.GetComponentsInChildren<Text>(true);
                for (int j = 0; j < texts.Length; j++)
                {
                    Text text = texts[j];
                    if (!LanguageManager.IsHaveChinese(text.text))
                    {
                        continue;
                    }
                    string value = text.text;
                    //如果在prefab中，限制了文字范围，那么ngui会在折行处添加\r，所以要去掉
                    value = value.Replace("\r", "");

                    if (value2RowDic.ContainsKey(value))
                    {
                        value2RowDic.Remove(value);
                    }
                }
            }

            //删除
            int index = 0;
            //行数从小到大排序,因为他这个删除是会自动把下一行移上来，所以需要特殊处理
            List<int> list = value2RowDic.Values.ToList();
            list.Sort((x, y) =>
            {
                return x.CompareTo(y);
            });
            foreach (var item in list)
            {
                sheet.DeleteRow(item - index);
                index++;
            }
            package.Save();
        }
        if (value2RowDic.Count == 0)
        {
            Debug.Log("无删除项");
        }
        else
        {
            Debug.Log("删除" + value2RowDic.Count + "项");
        }
        Debug.Log("完成");
    }
}
