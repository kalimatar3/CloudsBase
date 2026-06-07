
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ExcelDataReader;
using System;
public static class ExcelReader
{
    public static string[,] ReadExcel(string assetPath)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"❌ ExcelReader: File không tồn tại tại đường dẫn: {fullPath}");
                return null;
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(fullPath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet();
                if (dataSet.Tables.Count == 0)
                {
                    Debug.LogWarning($"⚠️ ExcelReader: File {assetPath} không có sheet nào.");
                    return null;
                }

                // Lấy sheet đầu tiên
                var table = dataSet.Tables[0];
                int rows = table.Rows.Count;
                int cols = table.Columns.Count;

                string[,] result = new string[rows, cols];

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        result[r,c] = table.Rows[r][c]?.ToString() ?? string.Empty;
                    }
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ ExcelReader lỗi: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }
}
