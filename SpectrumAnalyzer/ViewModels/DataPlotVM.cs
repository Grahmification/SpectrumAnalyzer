using System;
using System.Collections.Specialized;
using Microsoft.Win32;
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
            Data.PolyFit.FitEnableChanged += onEnableDataFitChanged;
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
                Color = OxyColors.Black,
                Title = "Raw Data",
                ItemsSource = Data.RawData,
            };

            var fitLineSeries = new LineSeries()
            {
                LineStyle = LineStyle.Dash,
                MarkerType = MarkerType.None,
                Color = OxyColors.Black,
                Title = "Polynomial Fit",
                ItemsSource = Data.FitCurveData,
            };

            var normalizedDataSeries = new LineSeries()
            {
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.Diamond,
                Color = OxyColors.Blue,
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
            onEnableDataFitChanged(this, Data.FitEnabled);
        }  
        public void onFFTCompleted(object sender, EventArgs e)
        {
            FFT = new FFTVM();
            FFT.ExportReconstructionComponentsRequest += onExportReconstructionComponents;
            FFT.ExportReconstructionPointsRequest += onExportReconstructionPoints;
            FFT.ExportReconstructionInterpolatedPointsRequest += onExportReconstructionInterpolatedPoints;

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


        public void onExportReconstructionComponents(object sender, SignalReconstructionVM recon)
        {
            var fd = new SaveFileDialog()
            {
                Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                CheckPathExists = true,
                Title = "Export Data",
                AddExtension = true,
                FileName = FormatExportFileName("Signal Components", recon)
            };

            if (fd.ShowDialog() == true && recon != null)
            {
                var writer = new CSVWriter(fd.FileName);

                writer.WriteLine(new string[] { "FFT Signal Reconstruction Components Data" });

                writer.WriteMetaData("Input Data Path", Data.DataFilePath);
                writer.WriteMetaData("Input Data Size", FFT.Dataset.Count.ToString());
                writer.WriteMetaData("Input Data Time Units", Units.SelectedXUnit.TimeString);
                writer.WriteMetaData("Input Data Frequency Units", Units.SelectedXUnit.FreqString);
                writer.WriteMetaData("Input Data Y Axis Units", Units.YAxisTitle);
                writer.WriteMetaData("Input Data Detrending?", Data.PolyFit.Enabled.ToString());
                writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                writer.WriteMetaData("Reconstruction Name", recon.Name);
                writer.WriteMetaData("Total FFT Components", FFT.SignalComponents.Count.ToString());
                writer.WriteMetaData("Reconstruction Components", recon.Function.Curves.Count.ToString());

                writer.WriteDataStartLine();
                writer.WriteLine(SignalComponent.GetExportHeader(Units.SelectedXUnit.FreqUnit, Units.SelectedXUnit.TimeUnit, Units.YAxisTitle));
                foreach (SignalComponent comp in recon.Function.Curves)
                    writer.WriteLine(comp.GetExportDataLine());

                writer.Close();
            }
        }
        public void onExportReconstructionPoints(object sender, SignalReconstructionVM recon)
        {

            var fd = new SaveFileDialog()
            {
                Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                CheckPathExists = true,
                Title = "Export Data",
                AddExtension = true,
                FileName = FormatExportFileName("Data Points", recon)
            };

            if (fd.ShowDialog() == true && recon != null)
            {
                var writer = new CSVWriter(fd.FileName);

                writer.WriteLine(new string[] { "FFT Signal Reconstruction Data Points" });

                writer.WriteMetaData("Input Data Path", Data.DataFilePath);
                writer.WriteMetaData("Input Data Size", FFT.Dataset.Count.ToString());
                writer.WriteMetaData("Input Data Time Units", Units.SelectedXUnit.TimeString);
                writer.WriteMetaData("Input Data Frequency Units", Units.SelectedXUnit.FreqString);
                writer.WriteMetaData("Input Data Y Axis Units", Units.YAxisTitle);
                writer.WriteMetaData("Input Data Detrending?", Data.PolyFit.Enabled.ToString());
                writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                writer.WriteMetaData("Reconstruction Name", recon.Name);
                writer.WriteMetaData("Total FFT Components", FFT.SignalComponents.Count.ToString());
                writer.WriteMetaData("Reconstruction Components", recon.Function.Curves.Count.ToString());

                writer.WriteDataStartLine();

                if (Data.FitEnabled)
                {
                    writer.WriteLine(new string[]
                        {
                            "#",
                        Units.SelectedXUnit.TimeString,
                        string.Format("Raw Data [{0}]",Units.YAxisTitle),
                        string.Format("Polynomial Detrend Curve [{0}]",Units.YAxisTitle),
                        string.Format("FFT Input Data [{0}]",Units.YAxisTitle),
                        string.Format("Reconstruction Data [{0}]",Units.YAxisTitle),
                        });

                    for (int i = 0; i < recon.NonInterpolatedPoints.Count; i++)
                        writer.WriteLine(new string[]
                        {
                        (i+1).ToString(),
                        recon.NonInterpolatedPoints[i].X.ToString(),
                        Data.RawData[i].Y.ToString(),
                        Data.FitCurveData[i].Y.ToString(),
                        Data.NormalizedData[i].Y.ToString(),
                        recon.NonInterpolatedPoints[i].Y.ToString()
                        });
                }
                else
                {
                    writer.WriteLine(new string[]
                       {
                            "#",
                        Units.SelectedXUnit.TimeString,
                        string.Format("FFT Input Data [{0}]",Units.YAxisTitle),
                        string.Format("Reconstruction Data [{0}]",Units.YAxisTitle),
                       });

                    for (int i = 0; i < recon.NonInterpolatedPoints.Count; i++)
                        writer.WriteLine(new string[]
                        {
                        (i+1).ToString(),
                        recon.NonInterpolatedPoints[i].X.ToString(),
                        Data.NormalizedData[i].Y.ToString(),
                        recon.NonInterpolatedPoints[i].Y.ToString()
                        });
                }

                writer.Close();
            }
        }
        public void onExportReconstructionInterpolatedPoints(object sender, SignalReconstructionVM recon)
        {

            var fd = new SaveFileDialog()
            {
                Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                CheckPathExists = true,
                Title = "Export Data",
                AddExtension = true,
                FileName = FormatExportFileName("Interpolated Data Points", recon)
            };

            if (fd.ShowDialog() == true && recon != null)
            {
                var writer = new CSVWriter(fd.FileName);

                writer.WriteLine(new string[] { "FFT Signal Reconstruction Interpolated Data Points" });

                writer.WriteMetaData("Input Data Path", Data.DataFilePath);
                writer.WriteMetaData("Input Data Size", FFT.Dataset.Count.ToString());
                writer.WriteMetaData("Input Data Time Units", Units.SelectedXUnit.TimeString);
                writer.WriteMetaData("Input Data Frequency Units", Units.SelectedXUnit.FreqString);
                writer.WriteMetaData("Input Data Y Axis Units", Units.YAxisTitle);
                writer.WriteMetaData("Input Data Detrending?", Data.PolyFit.Enabled.ToString());
                writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                writer.WriteMetaData("Reconstruction Name", recon.Name);
                writer.WriteMetaData("Total FFT Components", FFT.SignalComponents.Count.ToString());
                writer.WriteMetaData("Reconstruction Components", recon.Function.Curves.Count.ToString());
                writer.WriteMetaData("Interpolation Factor", recon.InterpolationFactor.ToString());

                writer.WriteDataStartLine();

                writer.WriteLine(new string[]
                    {
                        "#",
                    Units.SelectedXUnit.TimeString,
                    string.Format("Reconstruction Data [{0}]",Units.YAxisTitle),
                    });

                for (int i = 0; i < recon.Points.Count; i++)
                    writer.WriteLine(new string[]
                    {
                    (i+1).ToString(),
                    recon.Points[i].X.ToString(),
                    recon.Points[i].Y.ToString()
                    });
                
                writer.Close();
            }
        }

        private string FormatExportFileName(string fileType, SignalReconstructionVM recon)
        {
            var fName = string.Format("FFT {0} {1}", recon.Name, fileType);

            if (Units.DataTitle != "")
                fName = string.Format("{0} - {1}", Units.DataTitle, fName);

            return fName;
        }
    }
}
