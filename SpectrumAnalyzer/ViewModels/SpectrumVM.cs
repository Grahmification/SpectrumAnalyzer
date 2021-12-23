using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class SpectrumVM : ObservableObject
    {
        public ObservableCollection<SignalComponent> SignalComponents { get; set; } = new ObservableCollection<SignalComponent>();
        public SelectedItemCollection<SignalComponent> SelectedComponents { get; set; } = new SelectedItemCollection<SignalComponent>();
        public PlotModelManaged FrequencySpectrumPlot { get; set; } = new PlotModelManaged();
        public PlotModelManaged PeriodSpectrumPlot { get; set; } = new PlotModelManaged();


        private SignalComponent _selectedComponent = null;
        public SignalComponent SelectedComponent
        {
            get { return _selectedComponent; }
            set
            {
                _selectedComponent = value;
                //OnSignalComponentSelect();
            }
        }




        /// <summary>
        /// RelayCommand for <see cref="LoadData"/>
        /// </summary>
        public ICommand LoadDataCommand { get; private set; }

        public SpectrumVM()
        {
            SetupPlots();
            SelectedComponents.CollectionChanged += OnSignalComponentsSelected;
        }

        public void SetupPlots()
        {
            FrequencySpectrumPlot = new PlotModelManaged 
            { 
                Title = "FFT Frequency Spectrum"
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
                Title = "Highlighted Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5
            };

            FrequencySpectrumPlot.AddSeries(FFTFreqSpectrum, PlotSeriesTag.FFTSpectrumFrequency);
            FrequencySpectrumPlot.AddSeries(FFTFreqSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

            PeriodSpectrumPlot = new PlotModelManaged
            {
                Title = "FFT Period Spectrum"
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
                Title = "Highlighted Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5
            };

            PeriodSpectrumPlot.AddSeries(FFTPeriodSpectrum, PlotSeriesTag.FFTSpectrumPeriod);
            PeriodSpectrumPlot.AddSeries(FFTPeriodSpectrumHightlight, PlotSeriesTag.FFTSpectrumHighlight);

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

        private void OnSignalComponentSelect()
        {
            if (_selectedComponent != null)
            {
                FrequencySpectrumPlot.UpdateLineSeriesData(PlotSeriesTag.FFTSpectrumHighlight, new List<Datapoint>() { new Datapoint(_selectedComponent.Frequency, _selectedComponent.Magnitude) });
                FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, true);

                PeriodSpectrumPlot.UpdateLineSeriesData(PlotSeriesTag.FFTSpectrumHighlight, new List<Datapoint>() { new Datapoint(_selectedComponent.Period, _selectedComponent.Magnitude) });
                PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, true);
            }
            else
            {
                FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
                PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
            }

            FrequencySpectrumPlot.InvalidatePlot(true);
            PeriodSpectrumPlot.InvalidatePlot(true);
        }
        private void OnSignalComponentsSelected(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedComponents.Count > 0)
            {
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
            }
            else
            {
                FrequencySpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
                PeriodSpectrumPlot.SetSeriesVisibility(PlotSeriesTag.FFTSpectrumHighlight, false);
            }

            FrequencySpectrumPlot.InvalidatePlot(true);
            PeriodSpectrumPlot.InvalidatePlot(true);
        }


    }
}
