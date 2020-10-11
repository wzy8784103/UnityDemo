using System;
using System.Collections.Generic;
using OfficeOpenXml;
using System.IO;

public class ExcelTools
{
    #region Excel导出bytes
    public static byte[] Excel2Bytes(string excelPath, string sheetName)
    {
        ByteBuffer buffer = new ByteBuffer();

        FileInfo fileInfo = new FileInfo(excelPath);
        File.SetAttributes(excelPath, FileAttributes.Normal);
        using (ExcelPackage destPackage = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = destPackage.Workbook.Worksheets[sheetName];
            //这里需要重新计算一下行列，api的行列值空行有时候也会算入
            int rows = GetRealRows(worksheet);
            int cols = GetRealCols(worksheet);
            //缓存一下每一列的名字和类型，用下标当做key
            List<string> nameList = new List<string>();
            List<EDataType> typeList = new List<EDataType>();
            //这个类库行列都是从1开始算
            for (int c = 1; c <= cols; c++)
            {
                //第一行是名字
                nameList.Add(worksheet.Cells[1, c].Value.ToString());
                //第三行是type
                typeList.Add((EDataType)Enum.Parse(typeof(EDataType), worksheet.Cells[3, c].Value.ToString()));
            }
            //写入行列
            buffer.WriteInt(rows);
            buffer.WriteInt(cols);
            //写入name
            foreach(string name in nameList)
            {
                buffer.WriteString(name);
            }
            //写入type
            foreach (EDataType type in typeList)
            {
                buffer.WriteInt((int)type);
            }
            //写入数据,从左往右顺序写入
            for (int r = DataManager.startRow; r <= rows; r++)
            {
                for (int c = 1; c <= cols; c++)
                {
                    object value = worksheet.Cells[r, c].Value;
                    EDataType type = typeList[c - 1];
                    switch(type)
                    {
                        case EDataType.Int:
                            buffer.WriteInt(value == null ? 0 : int.Parse(value.ToString()));
                            break;
                        case EDataType.Long:
                            buffer.WriteLong(value == null ? 0 : long.Parse(value.ToString()));
                            break;
                        case EDataType.String:
                            buffer.WriteString(value == null ? "" : value.ToString());
                            break;
                        case EDataType.Float:
                            buffer.WriteFloat(value == null ? 0 : float.Parse(value.ToString()));
                            break;
                    }
                }
            }
        }
        return buffer.ToArray();
    }
    public static int GetRealRows(ExcelWorksheet worksheet)
    {
        int rows = worksheet.Dimension.Rows;
        for (int i = DataManager.startRow; i <= rows; i++)
        {
            if (worksheet.Cells[i, 1].Value == null)
            {
                rows = i - 1;
                break;
            }
        }
        return rows;
    }
    public static int GetRealCols(ExcelWorksheet worksheet)
    {
        int cols = worksheet.Dimension.Columns;
        for (int i = 1; i <= cols; i++)
        {
            if (worksheet.Cells[1, i].Value == null)
            {
                cols = i - 1;
                break;
            }
        }
        return cols;
    }
    #endregion
}
