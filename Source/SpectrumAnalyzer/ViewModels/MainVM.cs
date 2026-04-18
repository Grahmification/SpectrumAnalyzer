using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class MainVM : ObservableObject
    {
        public DataPlotVM Data { get; private set; }
        public ProjectVM Project { get; private set; }

        public string WindowTitle => Project.WindowTitle;

        public ICommand LoadDataCommand { get; private set; }

        public MainVM()
        {
            Data    = new DataPlotVM();
            Project = new ProjectVM(() => Data);

            // Propagate title changes up so the Window binding updates
            Project.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ProjectVM.WindowTitle))
                    OnPropertyChanged(nameof(WindowTitle));
            };

            // Mark project dirty whenever data state changes
            Data.StateChanged += (_, _) => Project.MarkDirty();

            // Handle New Project – reset the entire DataPlotVM
            Project.NewProjectRequested += OnNewProject;

            LoadDataCommand = new RelayCommand<object>(LoadData);
        }

        private void OnNewProject(object? sender, EventArgs e)
        {
            // Replace the DataPlotVM with a fresh one and re-wire events
            Data = new DataPlotVM();
            Data.StateChanged += (_, _) => Project.MarkDirty();

            // Update ProjectVM's reference via its factory func (already a closure)
            OnPropertyChanged(nameof(Data));
            OnPropertyChanged(nameof(WindowTitle));
        }

        public void LoadData(object parameter)
        {
            try
            {
                var fd = new OpenFileDialog()
                {
                    Filter       = "Excel files (*.xlsx)|*.xlsx|csv files (*.csv)|*.csv|All files (*.*)|*.*",
                    Multiselect  = false,
                    CheckPathExists = true,
                    Title        = "Load Dataset",
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
