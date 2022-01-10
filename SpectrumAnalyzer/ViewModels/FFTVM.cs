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

        public string ReconstructionPlotTitle { get { return Units.FormattedPlotTitle("Signal Reconstruction"); } }
        public string FrequencyPlotTitle { get { return Units.FormattedPlotTitle("FFT Frequency Spectrum");} }
        public string PeriodPlotTitle { get { return Units.FormattedPlotTitle("FFT Period Spectrum"); } }


        public PlotModelManaged FrequencySpectrumPlot { get; set; } = new PlotModelManaged();
        public PlotModelManaged PeriodSpectrumPlot { get; set; } = new PlotModelManaged();
        public PlotModelManaged ReconstructionPlot { get; set; } = new PlotModelManaged();

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
         
            FrequencySpectrumPlot = new PlotModelManaged
            {
                Title = FrequencyPlotTitle
            };

            FrequencySpectrumPlot.Axes.Add(PlotModelManaged.AxisXPrimaryData("Frequency"));
            FrequencySpectrumPlot.Axes.Add(PlotModelManaged.AxisYPrimaryData("Magnitude"));
            FrequencySpectrumPlot.Legends.Add(PlotModelManaged.DataLengend());

            var FFTFreqSpectrum = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "FFT Frequency Spectrum",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false
            };

            var FFTFreqSpectrumHightlight = new LineSeries()
            {
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                Title = "Selected Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5
            };

            FrequencySpectrumPlot.AddSeries(FFTFreqSpectrum, PlotSeriesTag.FFTSpectrumFrequency);
            FrequencySpectrumPlot.AddSeries(FFTFreqSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

            //-------------------- Period Plot --------------------------

            PeriodSpectrumPlot = new PlotModelManaged
            {
                Title = PeriodPlotTitle
            };

            PeriodSpectrumPlot.Axes.Add(PlotModelManaged.AxisXPrimaryData("Period"));
            PeriodSpectrumPlot.Axes.Add(PlotModelManaged.AxisYPrimaryData("Magnitude"));
            PeriodSpectrumPlot.Legends.Add(PlotModelManaged.DataLengend());

            var FFTPeriodSpectrum = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "FFT Period Spectrum",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false
            };

            var FFTPeriodSpectrumHightlight = new LineSeries()
            {
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                Title = "Selected Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5
            };

            PeriodSpectrumPlot.AddSeries(FFTPeriodSpectrum, PlotSeriesTag.FFTSpectrumPeriod);
            PeriodSpectrumPlot.AddSeries(FFTPeriodSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

            //-------------------- Reconstruction Plot --------------------------

            ReconstructionPlot = new PlotModelManaged()
            {
                Title = ReconstructionPlotTitle
            };

            ReconstructionPlot.Axes.Add(PlotModelManaged.AxisXPrimaryData());
            ReconstructionPlot.Axes.Add(PlotModelManaged.AxisYPrimaryData());
            ReconstructionPlot.Legends.Add(PlotModelManaged.DataLengend());

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

            ReconstructionPlot.AddSeries(DataSeries, PlotSeriesTag.RawData);
            ReconstructionPlot.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);
        }
        public void SetUnits(UnitsVM units)
        {
            Units = units;
            Units.OnUnitsUpdate += OnUnitsUpdate;
        }
        public void PopulateDataSet(IEnumerable<Datapoint> dataset)
        {
            Dataset.Clear();

            foreach (Datapoint point in dataset)
            {
                Dataset.Add(point);
            }

            ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            ReconstructionPlot.ResetAllAxes();
            ReconstructionPlot.InvalidatePlot(true);
        }
        public void PopulateComponents(IEnumerable<SignalComponent> components)
        {
            SelectedComponents.Clear();
            SignalComponents.Clear();

            foreach (SignalComponent component in components)
            {
                SignalComponents.Add(component);
            }

            var freqLine = (LineSeries)FrequencySpectrumPlot.PlotSeries[PlotSeriesTag.FFTSpectrumFrequency];
            var periodLine = (LineSeries)PeriodSpectrumPlot.PlotSeries[PlotSeriesTag.FFTSpectrumPeriod];

            freqLine.Points.Clear();
            periodLine.Points.Clear();

            foreach (SignalComponent component in components)
            {
                freqLine.Points.Add(new DataPoint(component.Frequency, component.Magnitude));

                if(component.Period != 0)
                    periodLine.Points.Add(new DataPoint(component.Period, component.Magnitude));

            }

            FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumFrequency, true);
            PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumPeriod, true);

            FrequencySpectrumPlot.ResetAllAxes();
            FrequencySpectrumPlot.InvalidatePlot(true);

            PeriodSpectrumPlot.ResetAllAxes();
            PeriodSpectrumPlot.InvalidatePlot(true);
         
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

            ReconstructionPlot.AddSeries(fitLineSeries, (PlotSeriesTag)(Reconstructions.Count + 200));
            ReconstructionPlot.SetSeriesVisibility((PlotSeriesTag)(Reconstructions.Count + 200), true);
            ReconstructionPlot.InvalidatePlot(true);
        }
        public bool AreSignalComponentsSelected()
        {
            return SelectedComponents.Count > 0;
        }

        private void OnSignalComponentsSelected(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedComponents.Count > 0)
            {
                //-------- Plot Selected Points ---------------------
                
                var freqData = new List<Datapoint>();
                var periodData = new List<Datapoint>();

                foreach(SignalComponent comp in SelectedComponents)
                {
                    freqData.Add(new Datapoint(comp.Frequency, comp.Magnitude));
                    periodData.Add(new Datapoint(comp.Period, comp.Magnitude));
                }

                FrequencySpectrumPlot.UpdateLineSeriesData(PlotSeriesTag.FFTSpectrumHighlight, freqData);
                FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, true);

                PeriodSpectrumPlot.UpdateLineSeriesData(PlotSeriesTag.FFTSpectrumHighlight, periodData);
                PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, true);
     
                //------------ Plot Reconstruction From Selected Components ---------------------

                SelectedReconstruction = new SignalReconstructionVM(SelectedComponents);
                SelectedReconstruction.PopulatePoints((List<double>)Dataset.XValues);
                ((LineSeries)ReconstructionPlot.PlotSeries[PlotSeriesTag.FitLine]).ItemsSource = SelectedReconstruction.Points;
                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, true);
            }
            else
            {
                FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
                PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
                ReconstructionPlot.SetSeriesVisibility(PlotSeriesTag.FitLine, false);
            }

            FrequencySpectrumPlot.InvalidatePlot(true);
            PeriodSpectrumPlot.InvalidatePlot(true);
            ReconstructionPlot.InvalidatePlot(true);
        }

        public void OnUnitsUpdate(object sender, EventArgs e)
        {
            FrequencySpectrumPlot.Title = FrequencyPlotTitle;
            FrequencySpectrumPlot.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = Units.SelectedXUnit.FreqString;
            FrequencySpectrumPlot.InvalidatePlot(false);

            PeriodSpectrumPlot.Title = PeriodPlotTitle;
            PeriodSpectrumPlot.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = Units.SelectedXUnit.TimeString;
            PeriodSpectrumPlot.InvalidatePlot(false);

            ReconstructionPlot.Title = ReconstructionPlotTitle;
            ReconstructionPlot.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = Units.XAxisTitle;
            ReconstructionPlot.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title = Units.YAxisTitle;
            ReconstructionPlot.InvalidatePlot(false);
        }

    }
}
