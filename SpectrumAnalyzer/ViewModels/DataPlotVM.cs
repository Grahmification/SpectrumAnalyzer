using System;
using OxyPlot;
using OxyPlot.Series;
using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataPlotVM : ObservableObject
    {
        public PlotModelManaged DataPlotModel { get; set; }      
        public string DataPlotTitle { get { return Units.FormattedPlotTitle("Data"); } }

        public DataVM Data { get; private set; } = new DataVM();
        public UnitsVM Units { get; set; } = new UnitsVM();
        public FFTVM FFT { get; private set; } = new FFTVM();

        public DataPlotVM()
        {
            Units.OnUnitsUpdate += OnUnitsUpdate;
            Data.FitCompleted += onFitCompleted;
            Data.FFTCompleted += onFFTCompleted;
            Data.FitEnableChanged += onEnableDataFitChanged;

            SetupPlot();
            SetupSeries();

            FFT.SetUnits(Units);
            Units.UpdateUnits(null);
        }
        private void SetupPlot()
        {
            DataPlotModel = new PlotModelManaged()
            {
                Title = DataPlotTitle
            };

            var XAxis = PlotModelManaged.AxisXPrimaryData();
            XAxis.Title = Units.XAxisTitle;

            var YAxis = PlotModelManaged.AxisYPrimaryData();
            YAxis.Title = Units.YAxisTitle;

            DataPlotModel.Axes.Add(XAxis);
            DataPlotModel.Axes.Add(YAxis);
            DataPlotModel.Legends.Add(PlotModelManaged.DataLengend());
        }
        private void SetupSeries()
        {
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

            DataPlotModel.AddSeries(rawDataSeries, PlotSeriesTag.RawData);
            DataPlotModel.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);
            DataPlotModel.AddSeries(normalizedDataSeries, PlotSeriesTag.NormalizedData);
        }

        public void SetData(double[] XData, double[] YData, string dataTitle)
        {
            Data.SetData(XData, YData);

            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            DataPlotModel.ResetAllAxes();
            DataPlotModel.InvalidatePlot(true);

            Data.ComputeFit(null);

            Units.PlotTitle = dataTitle;
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
        }
        public void onEnableDataFitChanged(object sender, bool enable)
        {
            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.FitLine, enable);
            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.NormalizedData, enable);
            DataPlotModel.InvalidatePlot(true);
        }
        public void OnUnitsUpdate(object sender, EventArgs e)
        {
            DataPlotModel.Title = DataPlotTitle;
            DataPlotModel.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = Units.XAxisTitle;
            DataPlotModel.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title = Units.YAxisTitle;
            DataPlotModel.InvalidatePlot(false);
        }

    }
}
