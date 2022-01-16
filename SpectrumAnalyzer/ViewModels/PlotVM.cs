using Microsoft.Win32;
using SpectrumAnalyzer.Models;
using System.Windows.Input;

namespace SpectrumAnalyzer.ViewModels
{
    public class PlotVM : ObservableObject
    {
        public PlotModelManaged Model { get; set; } = new PlotModelManaged();
        
        private string _titlePrefix  = "";
        public string TitlePrefix { get { return _titlePrefix; } set { _titlePrefix = value; UpdateTitle(); } }
        
        private string _titleSuffix = "";
        public string TitleSuffix { get { return _titleSuffix; } set { _titleSuffix = value; UpdateTitle(); } }
        public string Title { get { return TitleSuffix == "" ? TitlePrefix : string.Format("{0}: {1}", TitlePrefix, TitleSuffix); } }

        public string AxisTitlePrimaryX 
        {
            get { return Model.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title; } 
            set { Model.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = value; Model.InvalidatePlot(false); } 
        }
        public string AxisTitlePrimaryY
        {
            get { return Model.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title; }
            set { Model.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title = value; Model.InvalidatePlot(false); }
        }

        /// <summary>
        /// RelayCommand for <see cref="ResetZoom"/>
        /// </summary>
        public ICommand ResetZoomCommand { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="SaveImage"/>
        /// </summary>
        public ICommand SaveImageCommand { get; private set; }

        public PlotVM()
        {
            ResetZoomCommand = new RelayCommand<object>(ResetZoom);
            SaveImageCommand = new RelayCommand<object>(SaveImage);
            SetupModel();
        }
        private void SetupModel()
        {
            Model = new PlotModelManaged();
            Model.Axes.Add(PlotModelManaged.AxisXPrimaryData());
            Model.Axes.Add(PlotModelManaged.AxisYPrimaryData());
            Model.Legends.Add(PlotModelManaged.DataLengend());
        }

        public void UpdateAxisTitles(string XTitle, string YTitle)
        {
            Model.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = XTitle;
            Model.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title = YTitle;
            Model.InvalidatePlot(false);
        }
        public void ResetZoom(object parameter)
        {
            Model.ResetZoom();
        }
        public void SaveImage(object parameter)
        {
            var fd = new SaveFileDialog()
            {
                Filter = "PNG Image (*.png)|*.png|All files (*.*)|*.*",
                CheckPathExists = true,
                Title = "Save Image",
                AddExtension = true,
                FileName = Title
            };

            if (fd.ShowDialog() == true)
            {
                Model.SaveImage(fd.FileName);
            }
        }

        private void UpdateTitle()
        {
            Model.Title = Title;
            Model.InvalidatePlot(false);
        }
    }
}
