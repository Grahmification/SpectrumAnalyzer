using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Legends;
using SpectrumAnalyzer.Models;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataVM : ObservableObject
    {
        public PlotModelManaged DataPlotModel { get; set; }
        public Dataset Data { get; set; } = null;
        public int PolyFitOrder { get; set; } = 3;
        public bool EnableFit
        { 
            get { return _enableFit;  }
            set { SetEnableDataFit(value);  }
        }

        public string PlotTitle { get; set; } = "Data";

        public double[] PolyCoefs { get; private set; } = new double[] { };

        private bool _enableFit = true;

        public UnitsVM Units { get; set; } = new UnitsVM();
        public SpectrumVM Spectrum { get; private set; } = new SpectrumVM();
        public ReconstructionVM Reconstruction { get; private set; } = new ReconstructionVM();


        /// <summary>
        /// RelayCommand for <see cref="ComputeFFT"/>
        /// </summary>
        public ICommand ComputeFFTCommand { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="ComputePolyFit"/>
        /// </summary>
        public ICommand ComputePolyFitCommand { get; private set; }

        public DataVM()
        {
            ComputeFFTCommand = new RelayCommand<object>(ComputeFFT, isDataNotNull);
            ComputePolyFitCommand = new RelayCommand<object>(ComputePolyFit, isDataNotNull);
            SetupPlot();
            SetupSeries();


            Units = new UnitsVM();
            Units.OnUnitsUpdate += OnUnitsUpdate;
        }
        private void SetupPlot()
        {
            DataPlotModel = new PlotModelManaged()
            {
                Title = Units.PlotTitle
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
            var rawDataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Circle,
                Title = "Raw Data",
            };

            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Dash,
                MarkerType = MarkerType.None,
                Title = "Polynomial Fit",
            };

            var normalizedDataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Diamond,
                Title = "Normalized Data"
            };

            DataPlotModel.AddSeries(rawDataSeries, PlotSeriesTag.RawData);
            DataPlotModel.AddSeries(fitLineSeries, PlotSeriesTag.FitLine);
            DataPlotModel.AddSeries(normalizedDataSeries, PlotSeriesTag.NormalizedData);
        }

        public void SetData(Models.Dataset data)
        {
            Data = data;

            //https://github.com/ylatuya/oxyplot/blob/master/Source/Examples/ExampleLibrary/Examples/ItemsSourceExamples.cs
            LineSeries dataline = (LineSeries)DataPlotModel.PlotSeries[PlotSeriesTag.RawData];
            dataline.ItemsSource = Data.RawData;

            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.RawData, true);


            DataPlotModel.ResetAllAxes();
            DataPlotModel.InvalidatePlot(true);

            ComputePolyFit(null);  
        }
        public void ComputePolyFit(object parameter)
        {
            Data.ComputePolyFit(PolyFitOrder);

            DataPlotModel.UpdateLineSeriesData(PlotSeriesTag.FitLine, Data.PolyFitCurve.CurvePoints);
            DataPlotModel.UpdateLineSeriesData(PlotSeriesTag.NormalizedData, Data.NormalizedData);     
            DataPlotModel.InvalidatePlot(true);
            PolyCoefs = Data.PolyFit.Coefficients;
        }  
        public void ComputeFFT(object parameter)
        {
            if(EnableFit)
            {
                Data.ComputeFFT(Data.NormalizedData);
            }
            else
            {
                Data.ComputeFFT(Data.RawData);
            }

            Spectrum.PopulateComponents(Data.FFTData.Values);
            Reconstruction.PopulateComponents(Data.FFTData.Values);
            SetReconstructionDataset();
        }

        public void SetEnableDataFit(bool enable)
        {
            _enableFit = enable;
            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.FitLine, enable);
            DataPlotModel.SetSeriesVisibility(PlotSeriesTag.NormalizedData, enable);
            DataPlotModel.InvalidatePlot(true);
        }

        public bool isDataNotNull()
        {
            return !(Data == null);
        }

        public void SetReconstructionDataset()
        {
            if (EnableFit)
            {
                Reconstruction.PopulateDataSet(Data.NormalizedData);
            }
            else
            {
                Reconstruction.PopulateDataSet(Data.RawData);
            }
        }

        public void OnUnitsUpdate(object sender, EventArgs e)
        {
            DataPlotModel.Title = Units.PlotTitle;
            DataPlotModel.GetAxis(PlotModelManaged.XAxisPrimaryKey).Title = Units.XAxisTitle;
            DataPlotModel.GetAxis(PlotModelManaged.YAxisPrimaryKey).Title = Units.YAxisTitle;
            DataPlotModel.InvalidatePlot(false);
        }

    }
}
