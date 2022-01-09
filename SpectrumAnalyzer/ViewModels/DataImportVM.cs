using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataImportVM : ObservableObject
    {


        public SpreadSheet SpreadSheet { get; private set; }
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
        public double[] SelectedXData { get { return GetDataColumnList(SelectedSheet, XDataColumn - 1); } }
        public double[] SelectedYData { get { return GetDataColumnList(SelectedSheet, YDataColumn - 1); } }


        public bool MultipleSheetsExist { get { return SpreadSheet?.WorkSheets.Count() > 1; } }
        public bool DataHeaders { get; set; } = true;
        public int XDataColumn { get; set; } = 1;
        public int YDataColumn { get; set; } = 2;
        public int ImportRows { get { return (SpreadSheet != null) ? (DataHeaders ? SelectedSheet.Rows.Count - 1 : SelectedSheet.Rows.Count) : 0; } }


        public event EventHandler ImportDataRequest;

        /// <summary>
        /// RelayCommand for <see cref="ImportData"/>
        /// </summary>
        public ICommand ImportDataCommand { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="CloseWindow"/>
        /// </summary>
        public ICommand CloseWindowCommand { get; private set; }


        public DataImportVM(SpreadSheet spreadsheet)
        {          
            SpreadSheet = spreadsheet;
            SelectedSheetName = SpreadSheet.WorkSheetNames.FirstOrDefault();

            SetupObjects();
        }

        public DataImportVM()
        {
            SetupObjects();
        }

        public void SetupObjects()
        {
            ImportDataCommand = new RelayCommand<Window>(ImportData, ColumnSelectionValid);
            CloseWindowCommand = new RelayCommand<Window>(CloseWindow);
        }

        public void ImportData(Window parameter)
        {
            ImportDataRequest?.Invoke(this, new EventArgs());
            CloseWindow(parameter);
        }

        public void CloseWindow(Window parameter)
        {
            parameter.Close();
        }

        public bool ColumnSelectionValid()
        {
            if (SelectedSheet is null)
                return false;

            if (XDataColumn < 1 || YDataColumn < 1 || XDataColumn > SelectedSheet.Columns.Count || YDataColumn > SelectedSheet.Columns.Count || XDataColumn == YDataColumn)
                return false;

            return true;
        }

        private double[] GetDataColumnList(DataTable table, int column)
        {
            var tmp = new List<double>();

            if(table != null)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    //skip the first row if we have dataheaders
                    if (i == 0 && DataHeaders)
                        continue;

                    //parsing because type can either be double or string depending on if excel or csv
                    tmp.Add(double.Parse(table.Rows[i].Field<object>(column).ToString()));
                }
            }
           
            return tmp.ToArray();
        }
    }
}
