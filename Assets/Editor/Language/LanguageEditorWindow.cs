using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Reflection;

public class LanguageEditorWindow : EditorWindow
{
    private int selectTab;

    //项目的数据存放路径，这里就写死吧
    public static string projectBytesPath;

    private LanguageCodePanel codePanel;
    private LanguagePrefabPanel prefabPanel;
    private LanguageExcelPanel excelPanel;

    private void OnEnable()
    {
        projectBytesPath = Application.dataPath + "/Resources/Data";
        codePanel = new LanguageCodePanel();
        prefabPanel = new LanguagePrefabPanel();
        prefabPanel.Init();
        excelPanel = new LanguageExcelPanel();
    }

    private void OnGUI()
    {
       selectTab = GUILayout.Toolbar(selectTab, new string[] { "Code", "Prefab", "Excel" });
        //code
        if (selectTab == 0)
        {
            codePanel.DrawUI();
        }
        //prefab
        else if (selectTab == 1)
        {
            prefabPanel.DrawUI();
        }
        //excel
        else if (selectTab == 2)
        {
            excelPanel.DrawUI();
        }
    }

    /// <summary>
    /// 防止只读啥的
    /// </summary>
    /// <param name="path"></param>
    public void SetFileAttribute(string path)
    {
        File.SetAttributes(path, FileAttributes.Normal);
    }

    public static void InitSheet(ExcelWorksheet sheet)
    {
        Dictionary<string, string> languageDict = new Dictionary<string, string>();
        languageDict.Add("Key", "主键");
        languageDict.Add("Chinese", "中文");
        languageDict.Add("English", "英文");
        int i = 1;
        foreach (var kv in languageDict)
        {
            //填充值
            sheet.Cells[1, i].Value = kv.Key;
            sheet.Cells[2, i].Value = kv.Value;
            sheet.Cells[3, i].Value = "String";
            //填充颜色
            sheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[2, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[3, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#8497B0"));
            sheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#BFBFBF"));
            sheet.Cells[3, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#BFBFBF"));
            //填充边框
            sheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            sheet.Cells[2, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            sheet.Cells[3, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            i++;
        }
    }
}
