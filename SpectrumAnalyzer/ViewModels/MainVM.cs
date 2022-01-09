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
        public DataPlotVM Data { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="LoadData"/>
        /// </summary>
        public ICommand LoadDataCommand { get; private set; }

        public MainVM()
        {
            InitializeVMs();
        }

        private void InitializeVMs()
        {
            Data = new DataPlotVM();

            LoadDataCommand = new RelayCommand<object>(LoadData);
        }

        public void LoadData(object parameter)
        {
            var fd = new OpenFileDialog()
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|csv files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = true,
                CheckPathExists = true,
                Title = "Load Dataset",
                AddExtension = true
            };

            if (fd.ShowDialog() == true)
            {
                var vm = new DataImportVM(new SpreadSheet(fd.FileName));
                vm.ImportDataRequest += OnImportData;

                var importWindow = new DataImportWindow();
                importWindow.DataContext = vm;
                importWindow.Show();
            }
        }

        public void OnImportData(object sender, EventArgs e)
        {
            var vm = (DataImportVM)sender;

            Data.SetData(new Dataset(vm.SelectedXData, vm.SelectedYData, vm.SpreadSheet.FilePath));
            vm.ImportDataRequest -= OnImportData;
        }

    }
}
