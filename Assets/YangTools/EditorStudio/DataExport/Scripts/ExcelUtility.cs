﻿#if  UNITY_EDITOR
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

public class ExcelUtility
{
    /// <summary>
    /// 表格数据集合
    /// </summary>
    private DataSet mResultSet;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="excelFile">Excel file.</param>
    public ExcelUtility(string excelFile)
    {
        FileStream mStream = File.Open(excelFile, FileMode.Open, FileAccess.Read);
        IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
        mResultSet = mExcelReader.AsDataSet();
    }

    /// <summary>
    /// 转换为实体类列表
    /// </summary>
    public List<T> ConvertToList<T>(int fieldNameRow = 0, int dataStartRow = 1)
    {
        //Excel文件中是否存在数据表
        if (mResultSet.Tables.Count < 1) return null;
        //默认读取第一个数据表
        DataTable mSheet = mResultSet.Tables[0];
        //数据表内是否存在数据
        if (mSheet.Rows.Count < 1) return null;

        int rowCount = mSheet.Rows.Count;//行数
        int colCount = mSheet.Columns.Count;//列数
        List<T> dataList = new List<T>();//list列表
        //读取数据
        for (int i = dataStartRow; i < rowCount; i++)
        {
            Type type = typeof(T);
            //构造函数
            ConstructorInfo ct = type.GetConstructor(System.Type.EmptyTypes);
            //创建实例
            T target = (T)ct.Invoke(null);
            for (int j = 0; j < colCount; j++)
            {
                //读取第1行数据作为表头字段
                string field = mSheet.Rows[fieldNameRow][j].ToString();
                object value = mSheet.Rows[i][j];
                //设置属性值
                SetTargetProperty(target, field, value);
            }
            //添加至列表
            dataList.Add(target);
        }

        return dataList;
    }
    /// <summary>
    /// 设置目标实例的属性
    /// </summary>
    private void SetTargetProperty(object target, string propertyName, object propertyValue)
    {
        //获取类型
        Type mType = target.GetType();
        //获取字段集合
        FieldInfo[] fieldInfos = mType.GetFields();
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            if (fieldInfo.Name == propertyName)
            {
                fieldInfo.SetValue(target, Convert.ChangeType(propertyValue, fieldInfo.FieldType));
                break;
            }
        }
    }

    /// <summary>
    /// 转换为Json
    /// </summary>
    /// <param name="JsonPath">Json文件路径</param>
    /// <param name="Header">表头行数</param>
    public void ConvertToJson(string JsonPath, Encoding encoding)
    {
        for (int k = 0; k < mResultSet.Tables.Count; k++)
        {
            //默认读取第一个数据表
            DataTable mSheet = mResultSet.Tables[k];

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            //准备一个列表存储整个表的数据
            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

            //读取数据
            for (int i = 1; i < rowCount; i++)
            {
                //准备一个字典存储每一行的数据
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int j = 0; j < colCount; j++)
                {
                    //读取第1行数据作为表头字段
                    string field = mSheet.Rows[0][j].ToString();
                    //Key-Value对应
                    row[field] = mSheet.Rows[i][j];
                }

                //添加到表数据中
                table.Add(row);
            }

            //生成Json字符串
            string json = LitJson.JsonMapper.ToJson(table);
            //写入文件
            using (FileStream fileStream = new FileStream(JsonPath + "/" + mSheet.TableName + ".json", FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                {
                    textWriter.Write(json);
                }
            }
        }
    }

    /// <summary>
    /// 转换为CSV
    /// </summary>
    public void ConvertToCSV(string CSVPath, Encoding encoding)
    {
        //遍历子表
        for (int i = 0; i < mResultSet.Tables.Count; i++)
        {
            //默认读取第一个数据表
            DataTable mSheet = mResultSet.Tables[i];

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            //创建一个StringBuilder存储数据
            StringBuilder stringBuilder = new StringBuilder();

            //读取数据
            for (int k = 0; k < rowCount; k++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    //使用","分割每一个数值
                    stringBuilder.Append(mSheet.Rows[k][j] + ",");
                }
                //使用换行符分割每一行
                stringBuilder.Append("\r\n");
            }

            //写入文件
            using (FileStream fileStream = new FileStream(CSVPath + "/" + mSheet.TableName + ".csv", FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                {
                    textWriter.Write(stringBuilder.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 导出为Xml
    /// </summary>
    public void ConvertToSqlDB(string XmlFile)
    {
        //判断Excel文件中是否存在数据表
        if (mResultSet.Tables.Count < 1)
            return;

        for (int k = 0; k < mResultSet.Tables.Count; k++)
        {
            //默认读取第一个数据表
            DataTable mSheet = mResultSet.Tables[k];

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            //创建一个StringBuilder存储数据
            StringBuilder stringBuilder = new StringBuilder();
            //创建Xml文件头
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.Append("\r\n");
            //创建根节点
            stringBuilder.Append("<Table>");
            stringBuilder.Append("\r\n");
            //读取数据
            for (int i = 1; i < rowCount; i++)
            {
                //创建子节点
                stringBuilder.Append("  <Row>");
                stringBuilder.Append("\r\n");
                for (int j = 0; j < colCount; j++)
                {
                    stringBuilder.Append("   <" + mSheet.Rows[0][j].ToString() + ">");
                    stringBuilder.Append(mSheet.Rows[i][j].ToString());
                    stringBuilder.Append("</" + mSheet.Rows[0][j].ToString() + ">");
                    stringBuilder.Append("\r\n");
                }
                //使用换行符分割每一行
                stringBuilder.Append("  </Row>");
                stringBuilder.Append("\r\n");
            }
            //闭合标签
            stringBuilder.Append("</Table>");
            //写入文件
            using (FileStream fileStream = new FileStream(XmlFile + "/" + mSheet.TableName + ".xml", FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
                {
                    textWriter.Write(stringBuilder.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 导出为Xml
    /// </summary>
    public void ConvertToXml(string XmlFile)
    {
        //判断Excel文件中是否存在数据表
        if (mResultSet.Tables.Count < 1)
            return;

        for (int k = 0; k < mResultSet.Tables.Count; k++)
        {
            //默认读取第一个数据表
            DataTable mSheet = mResultSet.Tables[k];

            //读取数据表行数和列数
            int rowCount = mSheet.Rows.Count;
            int colCount = mSheet.Columns.Count;

            //创建一个StringBuilder存储数据
            StringBuilder stringBuilder = new StringBuilder();
            //创建Xml文件头
            stringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.Append("\r\n");
            //创建根节点
            stringBuilder.Append("<Table>");
            stringBuilder.Append("\r\n");
            //读取数据
            for (int i = 1; i < rowCount; i++)
            {
                //创建子节点
                stringBuilder.Append("  <Row>");
                stringBuilder.Append("\r\n");
                for (int j = 0; j < colCount; j++)
                {
                    stringBuilder.Append("   <" + mSheet.Rows[0][j].ToString() + ">");
                    stringBuilder.Append(mSheet.Rows[i][j].ToString());
                    stringBuilder.Append("</" + mSheet.Rows[0][j].ToString() + ">");
                    stringBuilder.Append("\r\n");
                }
                //使用换行符分割每一行
                stringBuilder.Append("  </Row>");
                stringBuilder.Append("\r\n");
            }
            //闭合标签
            stringBuilder.Append("</Table>");
            //写入文件
            using (FileStream fileStream = new FileStream(XmlFile + "/" + mSheet.TableName + ".xml", FileMode.Create, FileAccess.Write))
            {
                using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
                {
                    textWriter.Write(stringBuilder.ToString());
                }
            }
        }
    }
}
#endif
