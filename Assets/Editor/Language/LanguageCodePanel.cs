using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;
using System.Text;

public class LanguageCodePanel
{
    private const string excelName = "L_Code.xlsx";
    private const string sheetName = "MasterCodeLanguage";
    //存档key
    private const string outputSaveKey = "CodeOutputSaveKey";
    private const string inputSaveKey = "CodeInputSaveKey";
    
    public void DrawUI()
    {
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
        string filePath = outputPath + "/" + excelName;
        FileInfo fileInfo = new FileInfo(filePath);
        bool isChange = false;
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
            Dictionary<string, string> excelDict = GetCodeExcelDic(sheet, ref newRow);

            FieldInfo[] fieldInfos = typeof(LanguageSet).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                string keyName = fieldInfos[i].Name;
                string valueName = TransforEscape(fieldInfos[i].GetValue(null).ToString());
                if (excelDict.ContainsKey(keyName))
                {
                    //如果出现代码和excel的key相同，但是value不一致，则输出一条错误
                    //这里不能去随意替换，因为并不能确定哪方是对的
                    //比如翻译人员发现一个字写错了直接改了，虽然这种行为是禁止的，但是还是选择不相信
                    if (!excelDict[keyName].Equals(valueName))
                    {
                        Debug.LogError("excelDict[keyName]===" + excelDict[keyName]);
                        Debug.LogError("valueName===" + valueName);
                        Debug.LogError("keyName===" + keyName + "->这个主键的值变了");
                        Debug.LogError("--------------");
                    }
                }
                else
                {
                    if (newRow > sheet.Dimension.Rows)
                    {
                        sheet.InsertRow(newRow, 1);
                    }
                    sheet.Cells[newRow, 1].Value = keyName;
                    sheet.Cells[newRow, 2].Value = valueName;
                    for(int j = 1; j <= cols; j++)
                    {
                        sheet.Cells[newRow, j].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    newRow++;
                    isChange = true;
                }
            }
            package.Save();
        }
        if (!isChange)
        {
            Debug.Log("无新增项");
        }
        Debug.Log("成功");
    }

    /// <summary>
    /// 删除无用的行数
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
        string filePath = path + "/" + excelName;
        FileInfo fileInfo = new FileInfo(filePath);
        List<int> deleteRowList = new List<int>();
        using (ExcelPackage destPackage = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = destPackage.Workbook.Worksheets[sheetName];
            //代码中的key放入set
            HashSet<string> codeSet = new HashSet<string>();
            FieldInfo[] fieldInfos = typeof(LanguageSet).GetFields();
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                codeSet.Add(fieldInfos[i].Name);
            }
            
            for (int i = DataManager.startRow; i <= worksheet.Dimension.Rows; i++)
            {
                if (worksheet.Cells[i, 1].Value == null)
                {
                    break;
                }
                else
                {
                    string key = worksheet.Cells[i, 1].Value.ToString();
                    if(!codeSet.Contains(key))
                    {
                        deleteRowList.Add(i);
                    }
                }
            }

            //删除，每次删除下面的都会上来，所以这里特殊处理一下
            int deleteIndex = 0;
            foreach (var item in deleteRowList)
            {
                worksheet.DeleteRow(item - deleteIndex);
                deleteIndex++;
            }
            destPackage.Save();
        }
        if(deleteRowList.Count == 0)
        {
            Debug.Log("无删除项");
        }
        else
        {
            Debug.Log("删除" + deleteRowList.Count + "项");
        }
        Debug.Log("完成");
    }

    /// <summary>
    /// 根据excel返回一个key-中文的字典
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="newRow"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetCodeExcelDic(ExcelWorksheet worksheet, ref int newRow)
    {
        Dictionary<string, string> excelDict = new Dictionary<string, string>();

        for (int i = DataManager.startRow; i <= worksheet.Dimension.Rows; i++)
        {
            if (worksheet.Cells[i, 1].Value == null)
            {
                newRow = i;
                break;
            }
            string key = worksheet.Cells[i, 1].Value.ToString();
            if (excelDict.ContainsKey(key))
            {
                Debug.LogError("重复主键");
            }
            else
            {
                excelDict.Add(key, worksheet.Cells[i, 2].Value.ToString());
            }
        }
        return excelDict;
    }
    /// <summary>
    /// 有时会有换行需求，加\n，但是这在转到excel时，会转为换行，所以这里多加一个转义字符
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string TransforEscape(string s)
    {
        string[] strs = s.Split('\n');
        if (strs.Length == 1)
        {
            return s;
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0, count = strs.Length; i < count; i++)
        {
            sb.Append(strs[i]);
            if (i != count - 1)
            {
                sb.Append("\\n");
            }
        }
        return sb.ToString();
    }
}
