using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataImportVM : ObservableObject
    {
        public ExcelSpreadSheet SpreadSheet { get; private set; }
        public string SelectedSheetName { get; set; } = "";
        public DataTable SelectedSheet
        {
            get
            {
                if (SpreadSheet is null)
                    return null;

                return SpreadSheet.WorkSheets.ContainsKey(SelectedSheetName) ? SpreadSheet?.WorkSheets[SelectedSheetName] : null;
            }
        }

        public bool MultipleSheetsExist { get { return SpreadSheet?.WorkSheets.Count() > 1; } }
        public bool DataHeaders { get; set; } = true;
        public int XDataColumn { get; set; } = 1;
        public int YDataColumn { get; set; } = 2;
        public int ImportRows { get { return (SpreadSheet != null) ? (DataHeaders ? SelectedSheet.Rows.Count - 1 : SelectedSheet.Rows.Count) : 0; } }



        public DataImportVM(ExcelSpreadSheet spreadsheet)
        {
            SpreadSheet = spreadsheet;
            SelectedSheetName = SpreadSheet.WorkSheetNames.FirstOrDefault();
        }

        public DataImportVM()
        {

        }

        //public DataTable FirstSheet { get { return SpreadSheet?.WorkSheets.First(); } }
    }
}
