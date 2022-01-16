using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class FFTVM : ObservableObject
    {
        public DatapointCollection Dataset { get; set; } = new DatapointCollection();
        public ObservableCollection<SignalReconstructionVM> Reconstructions { get; set; } = new ObservableCollection<SignalReconstructionVM>();
        public SignalReconstructionVM SelectedReconstruction { get; set; } = null;
        public SignalReconstructionVM PreviewReconstruction { get; private set; } = new SignalReconstructionVM("Reconstruction Preview");

        public string NewReconstructionName { get; set; } = "Reconstruction 1";
        public ObservableCollection<SignalComponent> SignalComponents { get; set; } = new ObservableCollection<SignalComponent>();
        public SelectedItemCollection<SignalComponent> SelectedComponents { get; set; } = new SelectedItemCollection<SignalComponent>();

        public PlotVM FrequencySpectrumPlot { get; set; } = new PlotVM();
        public PlotVM PeriodSpectrumPlot { get; set; } = new PlotVM();
        public PlotVM ReconstructionPlot { get; set; } = new PlotVM();
        public UnitsVM Units { get; private set; } = new UnitsVM();

        public event EventHandler<SignalReconstructionVM> ExportReconstructionComponentsRequest;

        public event EventHandler<SignalReconstructionVM> ExportReconstructionPointsRequest;
        
        public event EventHandler<SignalReconstructionVM> ExportReconstructionInterpolatedPointsRequest;

        public ICommand AddReconstructionCommand { get; private set; }
        public ICommand DeleteReconstructionCommand { get; private set; }
        public ICommand ExportReconstructionComponentsCommand { get; private set; }
        public ICommand ExportReconstructionPointsCommand { get; private set; }
        public ICommand ExportReconstructionInterpolatedPointsCommand { get; private set; }
        public ICommand ExportAllComponentsCommand { get; private set; }

        public FFTVM()
        {
            AddReconstructionCommand = new RelayCommand<object>(AddReconstruction, AllowNewReconstruction);
            DeleteReconstructionCommand = new RelayCommand<object>(DeleteReconstruction, AreReconstructionsSelected);
            ExportReconstructionComponentsCommand = new RelayCommand<object>(s => ExportReconstructionComponentsRequest?.Invoke(this, SelectedReconstruction), AreReconstructionsSelected);
            ExportReconstructionPointsCommand = new RelayCommand<object>(s => ExportReconstructionPointsRequest?.Invoke(this, SelectedReconstruction), AreReconstructionsSelected);
            ExportReconstructionInterpolatedPointsCommand = new RelayCommand<object>(s => ExportReconstructionInterpolatedPointsRequest?.Invoke(this, SelectedReconstruction), AreReconstructionsSelected);
            ExportAllComponentsCommand = new RelayCommand<object>(ExportAllComponents, ()=> SignalComponents.Count >0);

            SelectedComponents.CollectionChanged += OnSignalComponentsSelected;

            PreviewReconstruction.InterpolationFactor = 5;

            SetupPlots();
        }
        public void SetupPlots()
        {
            //-------------------- Frequency Plot --------------------------

            FrequencySpectrumPlot.TitlePrefix = "FFT Frequency Spectrum";
            FrequencySpectrumPlot.AxisTitlePrimaryX = "Frequency";
            FrequencySpectrumPlot.AxisTitlePrimaryY = "Magnitude";

            var FFTFreqSpectrum = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "FFT Frequency Spectrum",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                ItemsSource = SignalComponents,
                DataFieldX = "Frequency",
                DataFieldY = "Magnitude"
            };

            var FFTFreqSpectrumHightlight = new LineSeries()
            {
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                Title = "Selected Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5,
                ItemsSource = SelectedComponents,
                DataFieldX = "Frequency",
                DataFieldY = "Magnitude",
            };

            FrequencySpectrumPlot.Model.AddSeries(FFTFreqSpectrum, PlotSeriesTag.FFTSpectrumFrequency);
            FrequencySpectrumPlot.Model.AddSeries(FFTFreqSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

            //-------------------- Period Plot --------------------------

            PeriodSpectrumPlot.TitlePrefix = "FFT Period Spectrum";
            PeriodSpectrumPlot.AxisTitlePrimaryX = "Period";
            PeriodSpectrumPlot.AxisTitlePrimaryY = "Magnitude";

            var FFTPeriodSpectrum = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "FFT Period Spectrum",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
            };

            var FFTPeriodSpectrumHightlight = new LineSeries()
            {
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                Title = "Selected Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5,
                ItemsSource = SelectedComponents,
                DataFieldX = "Period",
                DataFieldY = "Magnitude"
            };

            PeriodSpectrumPlot.Model.AddSeries(FFTPeriodSpectrum, PlotSeriesTag.FFTSpectrumPeriod);
            PeriodSpectrumPlot.Model.AddSeries(FFTPeriodSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

            //-------------------- Reconstruction Plot --------------------------

            ReconstructionPlot.TitlePrefix = "Signal Reconstruction";

            var DataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "Input Data",
                CanTrackerInterpolatePoints = false,
                ItemsSource = Dataset
            };

            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.None,
                Title = PreviewReconstruction.Name,
                CanTrackerInterpolatePoints = true,
                ItemsSource = PreviewReconstruction.Points
        };

            ReconstructionPlot.Model.AddSeries(DataSeries, PlotSeriesTag.RawData);
            ReconstructionPlot.Model.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);
        }
        public void SetUnits(UnitsVM units)
        {
            Units = units;
            Units.OnUnitsUpdate += OnUnitsUpdate;
            OnUnitsUpdate(this, new EventArgs());
        }
        public void PopulateDataSet(IEnumerable<Datapoint> dataset)
        {
            Dataset.Clear();

            foreach (Datapoint point in dataset)
            {
                Dataset.Add(point);
            }

            ReconstructionPlot.Model.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            ReconstructionPlot.ResetZoom(null);
            ReconstructionPlot.Model.InvalidatePlot(true);
        }
        public void PopulateComponents(IEnumerable<SignalComponent> components)
        {
            SelectedComponents.Clear();
            SignalComponents.Clear();

            foreach (SignalComponent component in components)
            {
                SignalComponents.Add(component);
            }

            var periodLine = (LineSeries)PeriodSpectrumPlot.Model.PlotSeries[PlotSeriesTag.FFTSpectrumPeriod];
            periodLine.Points.Clear();

            //must do this way because we cant plot the dc offset as being huge
            foreach (SignalComponent component in components)
            {
                if(component.Period != 0)
                    periodLine.Points.Add(new DataPoint(component.Period, component.Magnitude));
            }

            FrequencySpectrumPlot.Model.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumFrequency, true);
            PeriodSpectrumPlot.Model.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumPeriod, true);

            FrequencySpectrumPlot.ResetZoom(null);
            PeriodSpectrumPlot.ResetZoom(null);
            FrequencySpectrumPlot.Model.InvalidatePlot(true);
            PeriodSpectrumPlot.Model.InvalidatePlot(true);
        }

        public void AddReconstruction(object parameter)
        {
            var recon = new SignalReconstructionVM(NewReconstructionName);
            recon.InterpolationFactor = PreviewReconstruction.InterpolationFactor;
            recon.PopulateComponents(new List<SignalComponent>(SelectedComponents));
            recon.PopulatePoints((List<double>)Dataset.XValues);

            Reconstructions.Add(recon);
            NewReconstructionName = string.Format("Reconstruction {0}", Reconstructions.Count + 1);

            //plot the reconstruction
            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.None,
                CanTrackerInterpolatePoints = true,
                ItemsSource = recon.Points,
                Title = recon.Name,
            };

            ReconstructionPlot.Model.AddSeries(fitLineSeries, (PlotSeriesTag)(Reconstructions.Count + 200));
            ReconstructionPlot.Model.SetSeriesVisibility((PlotSeriesTag)(Reconstructions.Count + 200), true);
            ReconstructionPlot.Model.InvalidatePlot(true);
        }
        public void DeleteReconstruction(object parameter)
        {
            var recon = SelectedReconstruction;
            
            foreach (LineSeries series in ReconstructionPlot.Model.PlotSeries.Values)
            {
                if (series.Title == recon.Name)
                    ReconstructionPlot.Model.RemoveSeries(series);
            }

            Reconstructions.Remove(recon);
            ReconstructionPlot.Model.InvalidatePlot(true);
        }
        public bool AreSignalComponentsSelected()
        {
            return SelectedComponents.Count > 0;
        }
        public bool AreReconstructionsSelected()
        {
            return SelectedReconstruction != null;
        }
        public bool NewReconstructionNameUnique()
        {
            for (int i = 0; i < Reconstructions.Count; i++)
            {
                if (Reconstructions[i].Name == NewReconstructionName)
                    return false;
            }

            return true;
        }
        public bool AllowNewReconstruction()
        {
            return AreSignalComponentsSelected() && NewReconstructionNameUnique();
        }

        public void ExportAllComponents(object parameter)
        {
            var recon = new SignalReconstructionVM("Full Spectrum");
            recon.PopulateComponents(SignalComponents);
            recon.PopulatePoints((List<double>)Dataset.XValues);

            ExportReconstructionComponentsRequest?.Invoke(this, recon);
        }

        private void OnSignalComponentsSelected(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool selected = false;
            
            if (SelectedComponents.Count > 0)
            {
                selected = true;

                //------------ Plot Reconstruction From Selected Components ---------------------
                PreviewReconstruction.PopulateComponents(SelectedComponents);
                PreviewReconstruction.PopulatePoints((List<double>)Dataset.XValues);
            }

            FrequencySpectrumPlot.Model.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, selected);
            PeriodSpectrumPlot.Model.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, selected);
            ReconstructionPlot.Model.SetSeriesVisibility(PlotSeriesTag.FitLine, selected);

            FrequencySpectrumPlot.Model.InvalidatePlot(true);
            PeriodSpectrumPlot.Model.InvalidatePlot(true);
            ReconstructionPlot.Model.InvalidatePlot(true);
        }
        public void OnUnitsUpdate(object sender, EventArgs e)
        {
            FrequencySpectrumPlot.TitleSuffix = Units.DataTitle;
            FrequencySpectrumPlot.AxisTitlePrimaryX = Units.SelectedXUnit.FreqString;

            PeriodSpectrumPlot.TitleSuffix = Units.DataTitle;
            PeriodSpectrumPlot.AxisTitlePrimaryX = Units.SelectedXUnit.TimeString;

            ReconstructionPlot.TitleSuffix = Units.DataTitle;
            ReconstructionPlot.AxisTitlePrimaryX = Units.XAxisTitle;
            ReconstructionPlot.AxisTitlePrimaryY = Units.YAxisTitle;
        }

    }
}
