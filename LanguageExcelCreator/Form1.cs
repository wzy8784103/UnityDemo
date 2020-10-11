using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System.Threading;

namespace LanguageExcelCreator
{
    public partial class englishImport : Form
    {
        private const string saveFileName = "AssetPath.txt";
        private const string translateFileName = "L_Excel.xlsx";
        private const string chineseRecordFileName = "ChineseRecord.txt";

        public englishImport()
        {
            InitializeComponent();
            if (File.Exists(saveFileName))
            {
                List<string> list = LitJson.JsonMapper.ToObject<List<string>>(File.ReadAllText(saveFileName));
                sourceBox1.Text = list[0];
                sourceBox2.Text = list[1];
                englishBox1.Text = list[2];
                englishBox2.Text = list[3];
                destBox.Text = list[4];
            }
        }

        /// <summary>
        /// 生成翻译数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickCreate(object sender, EventArgs e)
        {
            //Dictionary<string, HashSet<string>> chineseRecordDict = new Dictionary<string, HashSet<string>>();
            //chineseRecordDict.Add("1", new HashSet<string>() { "a", "b"});
            //chineseRecordDict.Add("2", new HashSet<string>() { "5", "4" });
            //CreateChineseRecordTxt(chineseRecordDict);
            //MessageBox.Show("完成");
            //return;
            if (sourceBox1.Text == "" || sourceBox2.Text == "" || destBox.Text == "")
            {
                MessageBox.Show("路径不能为空");
                return;
            }
            createButton.Enabled = false;
            Thread.Sleep(1);
            //所有中文的字典
            Dictionary<string, string> chineseDict = new Dictionary<string, string>();
            Dictionary<string, HashSet<string>> chineseRecordDict = new Dictionary<string, HashSet<string>>();
            SetChineseDict(sourceBox1.Text, chineseDict, chineseRecordDict);
            SetChineseDict(sourceBox2.Text, chineseDict, chineseRecordDict);
            CreateChineseRecordTxt(chineseRecordDict);
            CreateTranslateExcel(chineseDict);
            createButton.Enabled = true;
        }

        /// <summary>
        /// 设置字典
        /// </summary>
        /// <param name="path">原始表路径</param>
        /// <param name="chineseDict">所有的中文集合</param>
        /// <param name="chineseRecordDict">记录下哪个表哪个字段有中文</param>
        private void SetChineseDict(string path, Dictionary<string, string> chineseDict, Dictionary<string, HashSet<string>> chineseRecordDict)
        {
            using (ExcelPackage sourcePackage = new ExcelPackage(new FileInfo(path)))
            {
                //统计所有表中的中文，放入字典中
                ExcelWorksheets sheets = sourcePackage.Workbook.Worksheets;
                foreach (ExcelWorksheet sheet in sheets)
                {
                    //排除两个sheet
                    if (sheet.Name == "列表" || sheet.Name == "样例" || sheet.Name == "MasterSystemVariable")
                    {
                        continue;
                    }
                    int rows = sheet.Dimension.Rows;
                    int cols = sheet.Dimension.Columns;
                    for (int j = 1; j <= cols; j++)
                    {
                        if(sheet.Cells[3, j].Value == null)
                        {
                            continue;
                        }
                        //第三行是值类型
                        string typeStr = sheet.Cells[3, j].Value.ToString();
                        //如果不是字符串则不作处理
                        if (!typeStr.StartsWith("varchar") && !typeStr.StartsWith("nvarchar"))
                        {
                            continue;
                        }

                        bool isHaveChinese = false;
                        //第4行开始为正式数据
                        for (int i = 4; i <= rows; i++)
                        {
                            //如果第一行就为空，则代表后面没数据了，直接break
                            if (sheet.Cells[i, 1].Value == null)
                            {
                                break;
                            }
                            //如果不是第一行为空，就continue
                            if (sheet.Cells[i, j].Value == null)
                            {
                                continue;
                            }
                            string value = sheet.Cells[i, j].Value.ToString();
                            if (IsHaveChinese(value))
                            {
                                isHaveChinese = true;
                                if (!chineseDict.ContainsKey(value))
                                {
                                    chineseDict.Add(value, sheet.Name);
                                }
                            }
                        }

                        if(isHaveChinese)
                        {
                            string byteName = sheet.Name + "Data";
                            if (!chineseRecordDict.ContainsKey(byteName))
                            {
                                chineseRecordDict.Add(byteName, new HashSet<string>());
                            }

                            chineseRecordDict[byteName].Add(sheet.Cells[1, j].Value.ToString());
                        }
                    }
                    //break;
                }
            }
        }

        private class ChineseRecordJson
        {
            public string name;
            public List<string> list;
        }

        /// <summary>
        /// 生成哪个表哪个字段有汉字的记录
        /// </summary>
        /// <param name="chineseRecordDict">所有汉字集合</param>
        private void CreateChineseRecordTxt(Dictionary<string, HashSet<string>> chineseRecordDict)
        {
            List<ChineseRecordJson> list = new List<ChineseRecordJson>();
            foreach(var kv in chineseRecordDict)
            {
                ChineseRecordJson record = new ChineseRecordJson();
                record.name = kv.Key;
                record.list = kv.Value.ToList();
                list.Add(record);
            }
            string chineseRecordPath = destBox.Text + "/" + chineseRecordFileName;
            //写文件
            File.WriteAllText(chineseRecordPath, LitJson.JsonMapper.ToJson(list));
        }
        /// <summary>
        /// 生成翻译表
        /// </summary>
        /// <param name="chineseDict">所有汉字集合</param>
        private void CreateTranslateExcel(Dictionary<string, string> chineseDict)
        {
            string tanslatePath = destBox.Text + "/" + translateFileName;
            //FileInfo newFile = new FileInfo(tanslatePath);

            //翻译表中的数据如果不在原始表中，则废弃掉，直接删除
            Dictionary<string, int> deleteDict = new Dictionary<string, int>();
            //多存一份字典吧，用来存所有key，然后生成key的时候用
            HashSet<string> translateSet = new HashSet<string>();
            //using (ExcelPackage destPackage = new ExcelPackage(newFile))

            //FileStream fileStream = File.Open(tanslatePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (ExcelPackage destPackage = new ExcelPackage(new FileInfo(tanslatePath)))
            {
                ExcelWorksheet sheet = null;
                if (destPackage.Workbook.Worksheets.Count == 0)
                {
                    sheet = destPackage.Workbook.Worksheets.Add("MasterExcelLanguage");
                    InitLanguageExcel(sheet);
                }
                else
                {
                    sheet = destPackage.Workbook.Worksheets["MasterExcelLanguage"];
                }
                int rows = sheet.Dimension.Rows;
                for (int i = 4; i <= rows; i++)
                {
                    if (sheet.Cells[i, 1].Value == null)
                    {
                        rows = i;
                        break;
                    }
                    string value = sheet.Cells[i, 2].Value.ToString();
                    //如果原始表中的中文翻译表中已经有了，则移除掉
                    if (chineseDict.ContainsKey(value))
                    {
                        chineseDict.Remove(value);
                        translateSet.Add(sheet.Cells[i, 1].Value.ToString());
                    }
                    else
                    {
                        if (!deleteDict.ContainsKey(value))
                        {
                            deleteDict.Add(value, i);
                        }
                    }
                }
                //删除多余项
                if (deleteDict.Count > 0)
                {
                    int index = 0;
                    //行数从小到大排序,因为他这个删除是会自动把下一行移上来，所以需要特殊处理
                    List<int> list = deleteDict.Values.ToList();
                    list.Sort((x, y) =>
                    {
                        return x.CompareTo(y);
                    });
                    foreach (var item in list)
                    {
                        sheet.DeleteRow(item - index);
                        index++;
                    }
                }

                //删除后要重新计算row
                int insetRow = rows - deleteDict.Count + 1;
                //此时chineseDict中的内容即为此次新增的内容
                if (chineseDict.Count == 0)
                {
                    MessageBox.Show("无新增项,删除" + deleteDict.Count + "项");
                    destPackage.Save();
                    return;
                }

                foreach (var kv in chineseDict)
                {
                    sheet.InsertRow(insetRow, 1);
                    //拿到不重复的key
                    int keyIndex = 1;
                    string key = GetKey(kv.Value, keyIndex);
                    while (translateSet.Contains(key))
                    {
                        keyIndex++;
                        key = GetKey(kv.Value, keyIndex);
                    }
                    translateSet.Add(key);
                    //填值
                    sheet.Cells[insetRow, 1].Value = key;
                    sheet.Cells[insetRow, 2].Value = kv.Key;
                    sheet.Cells[insetRow, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    sheet.Cells[insetRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    sheet.Cells[insetRow, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    insetRow++;
                }
                byte[] data = destPackage.GetAsByteArray();
                File.WriteAllBytes(tanslatePath, data);

                MessageBox.Show("新增" + chineseDict.Count + "项,删除" + deleteDict.Count + "项");
                //destPackage.Save();
            }
        }
        /// <summary>
        /// 初始化翻译表，加一些样式和初始化前三行数值
        /// </summary>
        /// <param name="sheet">sheet页</param>
        private void InitLanguageExcel(ExcelWorksheet sheet)
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
        /// <summary>
        /// 翻译表Key的存储形式
        /// </summary>
        /// <param name="excelName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetKey(string excelName, int index)
        {
            return excelName + "_" + index;
        }
        /// <summary>
        /// 是否含有中文
        /// </summary>
        /// <param name="str">待检测字符串</param>
        /// <returns></returns>
        private bool IsHaveChinese(string str)
        {
            foreach (char c in str)
            {
                if ((int)c > 127)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 三个路径选择都走一个，根据buttonname区分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickSelectPath(object sender, EventArgs e)
        {
            string buttonName = ((Button)sender).Name;
            //源文件路径
            if (buttonName == sourceButton1.Name)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    sourceBox1.Text = fileDialog.FileName;
                    SaveFilePath();
                }
            }
            else if(buttonName == sourceButton2.Name)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    sourceBox2.Text = fileDialog.FileName;
                    SaveFilePath();
                }
            }
            else if (buttonName == englishButton1.Name)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    englishBox1.Text = fileDialog.FileName;
                    SaveFilePath();
                }
            }
            else if (buttonName == englishButton2.Name)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    englishBox2.Text = fileDialog.FileName;
                    SaveFilePath();
                }
            }
            //目标路径
            else
            {
                FolderBrowserDialog pathDialog = new FolderBrowserDialog();
                if (pathDialog.ShowDialog() == DialogResult.OK)
                {
                    destBox.Text = pathDialog.SelectedPath;
                    SaveFilePath();
                }
            }
        }

        /// <summary>
        /// 本地保存路径
        /// </summary>
        private void SaveFilePath()
        {
            //写文件
            List<string> list = new List<string>();
            list.Add(sourceBox1.Text);
            list.Add(sourceBox2.Text);
            list.Add(englishBox1.Text);
            list.Add(englishBox2.Text);
            list.Add(destBox.Text);
            File.WriteAllText(saveFileName, LitJson.JsonMapper.ToJson(list));
        }

        private struct Dimension
        {
            public Dimension(int i, int j)
            {
                this.i = i;
                this.j = j;
            }

            public int i;
            public int j;
        }
        private void OnClickImportEnglish(object sender, EventArgs e)
        {
            if (sourceBox1.Text == "" || sourceBox2.Text == "" || destBox.Text == "" || englishBox1.Text == "" || englishBox2.Text == "")
            {
                MessageBox.Show("路径不能为空");
                return;
            }
            string tanslatePath = destBox.Text + "/" + translateFileName;
            if (!File.Exists(tanslatePath))
            {
                MessageBox.Show("请先生成翻译表");
                return;
            }
            importButton.Enabled = false;
            Thread.Sleep(1);
            //拿到所有中文的
            Dictionary<string, Dictionary<Dimension, string>> chineseDimensionDict = new Dictionary<string, Dictionary<Dimension, string>>();
            SetChineseExcel(sourceBox1.Text, OnSetChineseDimension, new List<object>() { chineseDimensionDict });
            SetChineseExcel(sourceBox2.Text, OnSetChineseDimension, new List<object>() { chineseDimensionDict });
            Dictionary<string, string> chinese2EnglishDict = new Dictionary<string, string>();
            SetChineseExcel(englishBox1.Text, OnSetChinese2English, new List<object>() { chineseDimensionDict, chinese2EnglishDict });
            SetChineseExcel(englishBox2.Text, OnSetChinese2English, new List<object>() { chineseDimensionDict, chinese2EnglishDict });

            //using (ExcelPackage destPackage = new ExcelPackage(new FileInfo(tanslatePath)))

            //FileInfo fileInfo = new FileInfo(tanslatePath);
            //fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //FileStream fileStream = File.Open(tanslatePath, FileMode.Open, FileAccess.Read);
            using (ExcelPackage destPackage = new ExcelPackage(new FileInfo(tanslatePath)))
            {
                ExcelWorksheet sheet = destPackage.Workbook.Worksheets["MasterExcelLanguage"];
                int rows = sheet.Dimension.Rows;
                for (int i = 4; i <= rows; i++)
                {
                    if (sheet.Cells[i, 1].Value == null)
                    {
                        break;
                    }
                    string value = sheet.Cells[i, 2].Value.ToString();
                    if (chinese2EnglishDict.ContainsKey(value))
                    {
                        sheet.Cells[i, 3].Value = chinese2EnglishDict[value];
                    }
                }

                //destPackage.SaveAs(fileStream);
                //destPackage.Dispose();
                //fileStream.Close();
                byte[] data = destPackage.GetAsByteArray();
                File.WriteAllBytes(tanslatePath, data);
                //byte[] data = destPackage.GetAsByteArray();
                //destPackage.SaveAs(fileStream);

                MessageBox.Show("导入完成");
            }
            importButton.Enabled = true;
        }

        private void OnSetChineseDimension(List<object> arg)
        {
            Dictionary<string, Dictionary<Dimension, string>> chineseDimensionDict = (Dictionary<string, Dictionary<Dimension, string>>)arg[0];
            ExcelWorksheet sheet = (ExcelWorksheet)arg[1];
            int i = (int)arg[2];
            int j = (int)arg[3];

            string value = sheet.Cells[i, j].Value.ToString();
            if (IsHaveChinese(value))
            {
                Dictionary<Dimension, string> dict = null;
                if (!chineseDimensionDict.ContainsKey(sheet.Name))
                {
                    dict = new Dictionary<Dimension, string>();
                    chineseDimensionDict.Add(sheet.Name, dict);
                }
                else
                {
                    dict = chineseDimensionDict[sheet.Name];
                }

                Dimension dimension = new Dimension(i, j);
                if (!dict.ContainsKey(dimension))
                {
                    dict.Add(dimension, value);
                }
            }
        }
        private void OnSetChinese2English(List<object> arg)
        {
            Dictionary<string, Dictionary<Dimension, string>> chineseDimensionDict = (Dictionary<string, Dictionary<Dimension, string>>)arg[0];
            Dictionary<string, string> chinese2EnglishDict = (Dictionary<string, string>)arg[1];
            ExcelWorksheet sheet = (ExcelWorksheet)arg[2];
            //如果这个表里没有中文，return
            if (!chineseDimensionDict.ContainsKey(sheet.Name))
            {
                return;
            }

            int i = (int)arg[3];
            int j = (int)arg[4];
            Dictionary<Dimension, string> dict = chineseDimensionDict[sheet.Name];
            Dimension dimension = new Dimension(i, j);
            if (dict.ContainsKey(dimension))
            {
                string chinese = dict[dimension];
                if (!chinese2EnglishDict.ContainsKey(chinese))
                {
                    chinese2EnglishDict.Add(chinese, sheet.Cells[i, j].Value.ToString());
                }
            }
        }

        private void SetChineseExcel(string path, Action<List<object>> action, List<object> arg)
        {
            using (ExcelPackage sourcePackage = new ExcelPackage(new FileInfo(path)))
            {
                //统计所有表中的中文，放入字典中
                ExcelWorksheets sheets = sourcePackage.Workbook.Worksheets;
                foreach (ExcelWorksheet sheet in sheets)
                {
                    //排除两个sheet
                    if (sheet.Name == "列表" || sheet.Name == "样例" || sheet.Name == "MasterSystemVariable")
                    {
                        continue;
                    }
                    int rows = sheet.Dimension.Rows;
                    int cols = sheet.Dimension.Columns;
                    for (int j = 1; j <= cols; j++)
                    {
                        if(sheet.Cells[3, j].Value == null)
                        {
                            continue;
                        }
                        //第三行是值类型
                        string typeStr = sheet.Cells[3, j].Value.ToString();
                        //如果不是字符串则不作处理
                        if (!typeStr.StartsWith("varchar") && !typeStr.StartsWith("nvarchar"))
                        {
                            continue;
                        }
                        //第4行开始为正式数据
                        for (int i = 4; i <= rows; i++)
                        {
                            //如果第一行就为空，则代表后面没数据了，直接break
                            if (sheet.Cells[i, 1].Value == null)
                            {
                                break;
                            }
                            //如果不是第一行为空，就continue
                            if (sheet.Cells[i, j].Value == null)
                            {
                                continue;
                            }
                            if(action != null)
                            {
                                List<object> temp = new List<object>();
                                temp.AddRange(arg);
                                temp.Add(sheet);
                                temp.Add(i);
                                temp.Add(j);
                                action(temp);
                            }
                        }
                    }
                    //break;
                }
            }
        }
    }
}
