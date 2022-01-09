using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class MainVM : ObservableObject
    {     
        public DataVM Data { get; private set; }

        public string DataFilePath { get; set; } = "";

        /// <summary>
        /// RelayCommand for <see cref="LoadData"/>
        /// </summary>
        public ICommand LoadDataCommand { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="LoadExcel"/>
        /// </summary>
        public ICommand LoadExcelCommand { get; private set; }


        public MainVM()
        {
            InitializeVMs();
        }

        private void InitializeVMs()
        {
            Data = new DataVM();

            LoadDataCommand = new RelayCommand<object>(LoadData);
            LoadExcelCommand = new RelayCommand<object>(LoadExcel);
        }

        public void LoadData(object parameter)
        {
            var fd = new OpenFileDialog()
            {
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = false,
                CheckPathExists = true,
                Title = "Load Dataset",
                AddExtension = true
            };

            if (fd.ShowDialog() == true)
            {              
                Data.SetData(new Dataset(fd.FileName));
                DataFilePath = fd.FileName;
            }
        }

        public void LoadExcel(object parameter)
        {
            var fd = new OpenFileDialog()
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|csv files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = false,
                CheckPathExists = true,
                Title = "Load Dataset",
                AddExtension = true
            };

            if (fd.ShowDialog() == true)
            {
                var vm = new DataImportVM(new ExcelSpreadSheet(fd.FileName));
                vm.ImportDataRequest += OnImportData;

                var importWindow = new DataImportWindow();
                importWindow.DataContext = vm;
                importWindow.Show();
            }
        }

        public void OnImportData(object sender, EventArgs e)
        {
            var vm = (DataImportVM)sender;

            Data.SetData(new Dataset(vm.SelectedXData, vm.SelectedYData));
            DataFilePath = vm.SpreadSheet.FilePath;

            vm.ImportDataRequest -= OnImportData;
        }

    }
}
