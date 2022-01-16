using System;
using System.Collections.Specialized;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataPlotVM : ObservableObject
    {
        public DataVM Data { get; private set; } = new DataVM();
        public UnitsVM Units { get; set; } = new UnitsVM();
        public FFTVM FFT { get; private set; } = new FFTVM();
        public PlotVM DataPlot { get; private set; } = new PlotVM();

        public DataPlotVM()
        {
            Units.OnUnitsUpdate += OnUnitsUpdate;
            Data.FitCompleted += onFitCompleted;
            Data.FFTCompleted += onFFTCompleted;
            Data.FitEnableChanged += onEnableDataFitChanged;
            Data.SelectedData.CollectionChanged += OnDataSelected;

            SetupPlot();

            FFT.SetUnits(Units);
            Units.UpdateUnits(null);
        }
        private void SetupPlot()
        {
            DataPlot.TitlePrefix = "Data";
            DataPlot.TitleSuffix = Units.DataTitle;
            DataPlot.AxisTitlePrimaryX = Units.XAxisTitle;
            DataPlot.AxisTitlePrimaryY = Units.YAxisTitle;

            //https://github.com/ylatuya/oxyplot/blob/master/Source/Examples/ExampleLibrary/Examples/ItemsSourceExamples.cs

            var rawDataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "Raw Data",
                ItemsSource = Data.RawData,
            };

            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Dash,
                MarkerType = MarkerType.None,
                Title = "Polynomial Fit",
                ItemsSource = Data.FitCurveData,
            };

            var normalizedDataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Diamond,
                Title = "Normalized Data",
                ItemsSource = Data.NormalizedData,
            };

            var dataHightlightSerires = new LineSeries()
            {
                LineStyle = LineStyle.None,
                MarkerType = MarkerType.Circle,
                Title = "Selected Point",
                YAxisKey = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color = OxyColors.Red,
                MarkerSize = 5,
                ItemsSource = Data.SelectedData
            };

            DataPlot.Model.AddSeries(rawDataSeries, PlotSeriesTag.RawData);
            DataPlot.Model.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);
            DataPlot.Model.AddSeries(normalizedDataSeries, PlotSeriesTag.NormalizedData);
            DataPlot.Model.AddSeries(dataHightlightSerires, PlotSeriesTag.SelectedSeries);
        }

        public void SetData(double[] XData, double[] YData, string dataTitle)
        {
            Data.SetData(XData, YData);

            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            DataPlot.ResetZoom(null);

            Data.ComputeFit(null);

            Units.DataTitle = dataTitle;
            Units.UpdateUnits(null);     
        }

        public void onFitCompleted(object sender, EventArgs e)
        {
            onEnableDataFitChanged(this, Data.EnableFit);
        }  
        public void onFFTCompleted(object sender, EventArgs e)
        {
            FFT = new FFTVM();
            FFT.PopulateComponents(Data.FFTData.Values);
            FFT.PopulateDataSet(Data.FFTInputData);
            FFT.SetUnits(Units);
        }
        public void onEnableDataFitChanged(object sender, bool enable)
        {
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.FitLine, enable);
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.NormalizedData, enable);
            DataPlot.Model.InvalidatePlot(true);
        }
        public void OnUnitsUpdate(object sender, EventArgs e)
        {
            DataPlot.TitleSuffix = Units.DataTitle;
            DataPlot.AxisTitlePrimaryX = Units.XAxisTitle;
            DataPlot.AxisTitlePrimaryY = Units.YAxisTitle;
        }
        private void OnDataSelected(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Data.SelectedData.Count > 0)
            {
                DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.SelectedSeries, true);
            }
            else
            {
                DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.SelectedSeries, false);
            }

            DataPlot.Model.InvalidatePlot(true);
        }

    }
}
