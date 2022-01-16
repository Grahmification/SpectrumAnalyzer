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
        public SignalReconstructionVM SelectedReconstruction { get; private set; } = new SignalReconstructionVM();

        public string NewReconstructionName { get; set; } = "Reconstruction 1";
        public ObservableCollection<SignalComponent> SignalComponents { get; set; } = new ObservableCollection<SignalComponent>();
        public SelectedItemCollection<SignalComponent> SelectedComponents { get; set; } = new SelectedItemCollection<SignalComponent>();

        public PlotVM FrequencySpectrumPlot { get; set; } = new PlotVM();
        public PlotVM PeriodSpectrumPlot { get; set; } = new PlotVM();
        public PlotVM ReconstructionPlot { get; set; } = new PlotVM();
        public UnitsVM Units { get; private set; } = new UnitsVM();

        public ICommand AddReconstructionCommand { get; private set; }

        public FFTVM()
        {
            AddReconstructionCommand = new RelayCommand<object>(AddReconstruction, AreSignalComponentsSelected);
            SelectedComponents.CollectionChanged += OnSignalComponentsSelected;

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
                MarkerType = MarkerType.Circle,
                Title = "Reconstruction",
                CanTrackerInterpolatePoints = true
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
            var recon = new SignalReconstructionVM(new List<SignalComponent>(SelectedComponents), NewReconstructionName);
            recon.PopulatePoints((List<double>)Dataset.XValues);

            Reconstructions.Add(recon);
            NewReconstructionName = string.Format("Reconstruction {0}", Reconstructions.Count + 1);

            //plot the reconstruction
            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                CanTrackerInterpolatePoints = true,
                ItemsSource = recon.Points,
                Title = recon.Name
            };

            ReconstructionPlot.Model.AddSeries(fitLineSeries, (PlotSeriesTag)(Reconstructions.Count + 200));
            ReconstructionPlot.Model.SetSeriesVisibility((PlotSeriesTag)(Reconstructions.Count + 200), true);
            ReconstructionPlot.Model.InvalidatePlot(true);
        }
        public bool AreSignalComponentsSelected()
        {
            return SelectedComponents.Count > 0;
        }

        private void OnSignalComponentsSelected(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool selected = false;
            
            if (SelectedComponents.Count > 0)
            {
                selected = true;

                //------------ Plot Reconstruction From Selected Components ---------------------
                SelectedReconstruction = new SignalReconstructionVM(SelectedComponents);
                SelectedReconstruction.PopulatePoints((List<double>)Dataset.XValues);
                ((LineSeries)ReconstructionPlot.Model.PlotSeries[PlotSeriesTag.FitLine]).ItemsSource = SelectedReconstruction.Points;
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
