using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpectrumAnalyzer.Models
{
    public class ExcelSpreadSheet
    {
        public Dictionary<string, DataTable> WorkSheets { get; private set; } = new Dictionary<string, DataTable>();
        public List<string> WorkSheetNames { get { return WorkSheets.Keys.ToList(); } }

        public ExcelSpreadSheet(string filePath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    //// reader.IsFirstRowAsColumnNames
                    var conf = new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = false
                        }
                    };

                    var result = reader.AsDataSet(conf);

                    foreach(DataTable table in result.Tables)
                        WorkSheets.Add(table.TableName, table);
                }
            }
        }

        /// <summary>
        /// Gets the value of a cell from a given worksheet
        /// </summary>
        /// <param name="WorkSheet"></param>
        /// <param name="cellIndex">The cell index string (ex. B7)</param>
        /// <returns></returns>
        public string GetCellValue(DataTable workSheet, string cellIndex)
        {
            //var cellStr = "AB2"; // var cellStr = "A1";
            var match = Regex.Match(cellIndex, @"(?<col>[A-Z]+)(?<row>\d+)");
            var colStr = match.Groups["col"].ToString();
            int col = (int)colStr.Select((t, i) => (colStr[i] - 64) * Math.Pow(26, colStr.Length - i - 1)).Sum();
            var row = int.Parse(match.Groups["row"].ToString());

            return workSheet.Rows[row-1][col-1].ToString();
        }

    }
}
