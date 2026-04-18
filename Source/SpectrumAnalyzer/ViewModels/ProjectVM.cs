using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SpectrumAnalyzer.Services;

namespace SpectrumAnalyzer.ViewModels
{
    public class ProjectVM : ObservableObject
    {
        private const string FileFilter = "Spectrum Analyzer Project (*.saproj)|*.saproj|All files (*.*)|*.*";
        private const string FileExtension = ".saproj";

        private string _currentFilePath = "";

        public string CurrentFilePath
        {
            get => _currentFilePath;
            private set
            {
                _currentFilePath = value;
                OnPropertyChanged(nameof(CurrentFilePath));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string WindowTitle =>
            string.IsNullOrEmpty(CurrentFilePath)
                ? "Spectrum Analyzer – Unsaved Project"
                : $"Spectrum Analyzer – {System.IO.Path.GetFileName(CurrentFilePath)}";

        public bool HasUnsavedChanges { get; private set; } = false;

        // The owning DataPlotVM – set once at construction time by MainVM.
        private readonly Func<DataPlotVM> _getDataPlotVM;

        // Raises when the entire data model should be reset (New Project).
        public event EventHandler? NewProjectRequested;

        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand SaveProjectAsCommand { get; }

        public ProjectVM(Func<DataPlotVM> getDataPlotVM)
        {
            _getDataPlotVM = getDataPlotVM;

            NewProjectCommand    = new RelayCommand<object>(_ => NewProject());
            OpenProjectCommand   = new RelayCommand<object>(_ => OpenProject());
            SaveProjectCommand   = new RelayCommand<object>(_ => SaveProject());
            SaveProjectAsCommand = new RelayCommand<object>(_ => SaveProjectAs());
        }

        // -----------------------------------------------------------------------
        // Called by the VM layer whenever something changes (data loaded, FFT run,
        // reconstruction added, etc.) so the title can show unsaved state.
        // -----------------------------------------------------------------------
        public void MarkDirty()
        {
            HasUnsavedChanges = true;
            OnPropertyChanged(nameof(WindowTitle));
        }

        // -----------------------------------------------------------------------
        // Commands
        // -----------------------------------------------------------------------
        private void NewProject()
        {
            if (!ConfirmDiscardChanges()) return;

            CurrentFilePath   = "";
            HasUnsavedChanges = false;
            NewProjectRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenProject()
        {
            if (!ConfirmDiscardChanges()) return;

            var fd = new OpenFileDialog
            {
                Filter          = FileFilter,
                CheckFileExists = true,
                Title           = "Open Project"
            };

            if (fd.ShowDialog() != true) return;

            try
            {
                ProjectFileService.Load(fd.FileName, _getDataPlotVM());
                CurrentFilePath   = fd.FileName;
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open project.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveProject()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
                SaveProjectAs();
            else
                DoSave(CurrentFilePath);
        }

        private void SaveProjectAs()
        {
            var fd = new SaveFileDialog
            {
                Filter          = FileFilter,
                DefaultExt      = FileExtension,
                CheckPathExists = true,
                Title           = "Save Project As",
                AddExtension    = true,
                FileName        = string.IsNullOrEmpty(CurrentFilePath)
                                    ? "Project"
                                    : System.IO.Path.GetFileNameWithoutExtension(CurrentFilePath)
            };

            if (fd.ShowDialog() == true)
                DoSave(fd.FileName);
        }

        private void DoSave(string filePath)
        {
            try
            {
                ProjectFileService.Save(filePath, _getDataPlotVM());
                CurrentFilePath   = filePath;
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not save project.\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // -----------------------------------------------------------------------
        private bool ConfirmDiscardChanges()
        {
            if (!HasUnsavedChanges) return true;

            var result = MessageBox.Show(
                "The current project has unsaved changes. Do you want to discard them?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }
    }
}
