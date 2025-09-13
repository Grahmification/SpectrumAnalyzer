using System.Windows;
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
            Data = new DataPlotVM();

            LoadDataCommand = new RelayCommand<object>(LoadData);
        }

        public void LoadData(object parameter)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the file. An Error Occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void OnImportData(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                var vm = (DataImportVM)sender;

                Data.SetData(vm.SelectedXData, vm.SelectedYData, vm.SpreadSheet?.FileName ?? "");
                Data.Data.DataFilePath = vm.SpreadSheet?.FilePath ?? "";
                vm.ImportDataRequest -= OnImportData;
            }
        }
    }
}
