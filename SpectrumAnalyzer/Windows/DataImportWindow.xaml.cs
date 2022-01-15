using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpectrumAnalyzer
{
    /// <summary>
    /// Interaction logic for DataImportWindow.xaml
    /// </summary>
    public partial class DataImportWindow : Window
    {
        public DataImportWindow()
        {
            InitializeComponent();
        }


        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
                
                e.Column.Header = dg.Columns.Count + 1;
            e.Column.Width = DataGridLength.Auto;


        }

        private void DataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            e.Row.Header = e.Row.AlternationIndex +1;
        }


    }
}
