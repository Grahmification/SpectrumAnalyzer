using System.Collections.Specialized;
using System.Windows;
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

        // Raised whenever meaningful state changes so ProjectVM can mark dirty
        public event EventHandler? StateChanged;

        public DataPlotVM()
        {
            Units.OnUnitsUpdate      += OnUnitsUpdate;
            Data.FitCompleted        += onFitCompleted;
            Data.FFTCompleted        += onFFTCompleted;
            Data.PolyFit.FitEnableChanged += onEnableDataFitChanged;
            Data.SelectedData.CollectionChanged += OnDataSelected;

            SetupPlot();

            FFT.SetUnits(Units);
            Units.UpdateUnits(null);
        }

        private void SetupPlot()
        {
            DataPlot.TitlePrefix      = "Data";
            DataPlot.TitleSuffix      = Units.DataTitle;
            DataPlot.AxisTitlePrimaryX = Units.XAxisTitle;
            DataPlot.AxisTitlePrimaryY = Units.YAxisTitle;

            var rawDataSeries = new LineSeries()
            {
                LineStyle   = LineStyle.Solid,
                MarkerType  = MarkerType.Circle,
                Color       = OxyColors.Black,
                Title       = "Raw Data",
                ItemsSource = Data.RawData,
            };
            var fitLineSeries = new LineSeries()
            {
                LineStyle   = LineStyle.Dash,
                MarkerType  = MarkerType.None,
                Color       = OxyColors.Black,
                Title       = "Polynomial Fit",
                ItemsSource = Data.FitCurveData,
            };
            var normalizedDataSeries = new LineSeries()
            {
                LineStyle   = LineStyle.Solid,
                MarkerType  = MarkerType.Diamond,
                Color       = OxyColors.Blue,
                Title       = "Normalized Data",
                ItemsSource = Data.NormalizedData,
            };
            var dataHightlightSeries = new LineSeries()
            {
                LineStyle                  = LineStyle.None,
                MarkerType                 = MarkerType.Circle,
                Title                      = "Selected Point",
                YAxisKey                   = "Primary Y",
                CanTrackerInterpolatePoints = false,
                Color                      = OxyColors.Red,
                MarkerSize                 = 5,
                ItemsSource                = Data.SelectedData
            };

            DataPlot.Model.AddSeries(rawDataSeries,          PlotSeriesTag.RawData);
            DataPlot.Model.AddSeries(fitLineSeries,           PlotSeriesTag.FitLine);
            DataPlot.Model.AddSeries(normalizedDataSeries,    PlotSeriesTag.NormalizedData);
            DataPlot.Model.AddSeries(dataHightlightSeries,    PlotSeriesTag.SelectedSeries);
        }

        // -----------------------------------------------------------------------
        // Normal data-import path
        // -----------------------------------------------------------------------
        public void SetData(double[] XData, double[] YData, string dataTitle)
        {
            Data.SetData(XData, YData);

            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            DataPlot.ResetZoom(null);

            Data.ComputeFit(null);

            Units.DataTitle = dataTitle;
            Units.UpdateUnits(null);

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        // -----------------------------------------------------------------------
        // Project-load helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Called by ProjectFileService after raw data and poly-fit have been
        /// loaded so the plot series become visible and axes reset.
        /// </summary>
        public void RefreshAfterLoad()
        {
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.RawData, true);
            DataPlot.ResetZoom(null);
            onEnableDataFitChanged(this, Data.FitEnabled);
            Units.UpdateUnits(null);
        }

        /// <summary>
        /// Called by ProjectFileService after FFT data has been injected via
        /// <see cref="DataVM.LoadFFTDirect"/>.  Mirrors the normal onFFTCompleted
        /// path but without re-creating the FFTVM (so the caller can still
        /// populate reconstructions afterwards).
        /// </summary>
        public void OnFFTCompletedFromLoad()
        {
            // Rebuild the FFTVM the same way onFFTCompleted does.
            FFT = new FFTVM();
            FFT.ExportReconstructionComponentsRequest    += onExportReconstructionComponents;
            FFT.ExportReconstructionPointsRequest        += onExportReconstructionPoints;
            FFT.ExportReconstructionInterpolatedPointsRequest += onExportReconstructionInterpolatedPoints;

            FFT.PopulateComponents(Data.FFTData.Values);
            FFT.PopulateDataSet(Data.FFTInputData);
            FFT.SetUnits(Units);
        }

        // -----------------------------------------------------------------------
        // Standard event handlers (unchanged)
        // -----------------------------------------------------------------------
        public void onFitCompleted(object? sender, EventArgs e)
        {
            onEnableDataFitChanged(this, Data.FitEnabled);
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void onFFTCompleted(object? sender, EventArgs e)
        {
            FFT = new FFTVM();
            FFT.ExportReconstructionComponentsRequest    += onExportReconstructionComponents;
            FFT.ExportReconstructionPointsRequest        += onExportReconstructionPoints;
            FFT.ExportReconstructionInterpolatedPointsRequest += onExportReconstructionInterpolatedPoints;

            FFT.PopulateComponents(Data.FFTData.Values);
            FFT.PopulateDataSet(Data.FFTInputData);
            FFT.SetUnits(Units);

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void onEnableDataFitChanged(object? sender, bool enable)
        {
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.FitLine,          enable);
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.NormalizedData,    enable);
            DataPlot.Model.InvalidatePlot(true);
        }

        public void OnUnitsUpdate(object? sender, EventArgs e)
        {
            DataPlot.TitleSuffix       = Units.DataTitle;
            DataPlot.AxisTitlePrimaryX = Units.XAxisTitle;
            DataPlot.AxisTitlePrimaryY = Units.YAxisTitle;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDataSelected(object? sender, NotifyCollectionChangedEventArgs e)
        {
            DataPlot.Model.SetSeriesVisibility(PlotSeriesTag.SelectedSeries, Data.SelectedData.Count > 0);
            DataPlot.Model.InvalidatePlot(true);
        }

        // -----------------------------------------------------------------------
        // Export handlers (unchanged)
        // -----------------------------------------------------------------------
        public void onExportReconstructionComponents(object? sender, SignalReconstructionVM? recon)
        {
            if (recon == null) return;
            try
            {
                var fd = new SaveFileDialog()
                {
                    Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                    CheckPathExists = true, Title = "Export Data",
                    AddExtension = true,
                    FileName = FormatExportFileName("Signal Components", recon)
                };

                if (fd.ShowDialog() == true)
                {
                    var writer = new CSVWriter(fd.FileName);
                    writer.WriteLine(["FFT Signal Reconstruction Components Data"]);
                    writer.WriteMetaData("Input Data Path",                     Data.DataFilePath);
                    writer.WriteMetaData("Input Data Size",                     FFT.Dataset.Count.ToString());
                    writer.WriteMetaData("Input Data Time Units",               Units.SelectedXUnit.TimeString);
                    writer.WriteMetaData("Input Data Frequency Units",          Units.SelectedXUnit.FreqString);
                    writer.WriteMetaData("Input Data Y Axis Units",             Units.YAxisTitle);
                    writer.WriteMetaData("Input Data Detrending?",              Data.PolyFit.Enabled.ToString());
                    writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                    writer.WriteMetaData("Reconstruction Name",                 recon.Name);
                    writer.WriteMetaData("Total FFT Components",                FFT.SignalComponents.Count.ToString());
                    writer.WriteMetaData("Reconstruction Components",           recon.Function.Curves.Count.ToString());
                    writer.WriteDataStartLine();
                    writer.WriteLine(SignalComponent.GetExportHeader(Units.SelectedXUnit.FreqUnit, Units.SelectedXUnit.TimeUnit, Units.YAxisTitle));
                    foreach (SignalComponent comp in recon.Function.Curves)
                        writer.WriteLine(comp.GetExportDataLine());
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export reconstruction. An Error Occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void onExportReconstructionPoints(object? sender, SignalReconstructionVM? recon)
        {
            if (recon == null) return;
            try
            {
                var fd = new SaveFileDialog()
                {
                    Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                    CheckPathExists = true, Title = "Export Data",
                    AddExtension = true,
                    FileName = FormatExportFileName("Data Points", recon)
                };

                if (fd.ShowDialog() == true)
                {
                    var writer = new CSVWriter(fd.FileName);
                    writer.WriteLine(["FFT Signal Reconstruction Data Points"]);
                    writer.WriteMetaData("Input Data Path",                     Data.DataFilePath);
                    writer.WriteMetaData("Input Data Size",                     FFT.Dataset.Count.ToString());
                    writer.WriteMetaData("Input Data Time Units",               Units.SelectedXUnit.TimeString);
                    writer.WriteMetaData("Input Data Frequency Units",          Units.SelectedXUnit.FreqString);
                    writer.WriteMetaData("Input Data Y Axis Units",             Units.YAxisTitle);
                    writer.WriteMetaData("Input Data Detrending?",              Data.PolyFit.Enabled.ToString());
                    writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                    writer.WriteMetaData("Reconstruction Name",                 recon.Name);
                    writer.WriteMetaData("Total FFT Components",                FFT.SignalComponents.Count.ToString());
                    writer.WriteMetaData("Reconstruction Components",           recon.Function.Curves.Count.ToString());
                    writer.WriteDataStartLine();

                    if (Data.FitEnabled)
                    {
                        writer.WriteLine(["#", Units.SelectedXUnit.TimeString,
                            $"Raw Data [{Units.YAxisTitle}]",
                            $"Polynomial Detrend Curve [{Units.YAxisTitle}]",
                            $"FFT Input Data [{Units.YAxisTitle}]",
                            $"Reconstruction Data [{Units.YAxisTitle}]"]);
                        for (int i = 0; i < recon.NonInterpolatedPoints.Count; i++)
                            writer.WriteLine([(i+1).ToString(), recon.NonInterpolatedPoints[i].X.ToString(),
                                Data.RawData[i].Y.ToString(), Data.FitCurveData[i].Y.ToString(),
                                Data.NormalizedData[i].Y.ToString(), recon.NonInterpolatedPoints[i].Y.ToString()]);
                    }
                    else
                    {
                        writer.WriteLine(["#", Units.SelectedXUnit.TimeString,
                            $"FFT Input Data [{Units.YAxisTitle}]",
                            $"Reconstruction Data [{Units.YAxisTitle}]"]);
                        for (int i = 0; i < recon.NonInterpolatedPoints.Count; i++)
                            writer.WriteLine([(i+1).ToString(), recon.NonInterpolatedPoints[i].X.ToString(),
                                Data.NormalizedData[i].Y.ToString(), recon.NonInterpolatedPoints[i].Y.ToString()]);
                    }
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export reconstruction. An Error Occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void onExportReconstructionInterpolatedPoints(object? sender, SignalReconstructionVM? recon)
        {
            if (recon == null) return;
            try
            {
                var fd = new SaveFileDialog()
                {
                    Filter = "CSV File (*.csv)|*.csv|All files (*.*)|*.*",
                    CheckPathExists = true, Title = "Export Data",
                    AddExtension = true,
                    FileName = FormatExportFileName("Interpolated Data Points", recon)
                };

                if (fd.ShowDialog() == true)
                {
                    var writer = new CSVWriter(fd.FileName);
                    writer.WriteLine(["FFT Signal Reconstruction Interpolated Data Points"]);
                    writer.WriteMetaData("Input Data Path",                     Data.DataFilePath);
                    writer.WriteMetaData("Input Data Size",                     FFT.Dataset.Count.ToString());
                    writer.WriteMetaData("Input Data Time Units",               Units.SelectedXUnit.TimeString);
                    writer.WriteMetaData("Input Data Frequency Units",          Units.SelectedXUnit.FreqString);
                    writer.WriteMetaData("Input Data Y Axis Units",             Units.YAxisTitle);
                    writer.WriteMetaData("Input Data Detrending?",              Data.PolyFit.Enabled.ToString());
                    writer.WriteMetaData("Detrending Poly Coefficients [x^0...x^n]", Data.PolyFit.PolyCoefsString);
                    writer.WriteMetaData("Reconstruction Name",                 recon.Name);
                    writer.WriteMetaData("Total FFT Components",                FFT.SignalComponents.Count.ToString());
                    writer.WriteMetaData("Reconstruction Components",           recon.Function.Curves.Count.ToString());
                    writer.WriteMetaData("Interpolation Factor",               recon.InterpolationFactor.ToString());
                    writer.WriteDataStartLine();
                    writer.WriteLine(["#", Units.SelectedXUnit.TimeString, $"Reconstruction Data [{Units.YAxisTitle}]"]);
                    for (int i = 0; i < recon.Points.Count; i++)
                        writer.WriteLine([(i+1).ToString(), recon.Points[i].X.ToString(), recon.Points[i].Y.ToString()]);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export reconstruction. An Error Occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string FormatExportFileName(string fileType, SignalReconstructionVM recon)
        {
            var fName = $"FFT {recon.Name} {fileType}";
            if (Units.DataTitle != "")
                fName = $"{Units.DataTitle} - {fName}";
            return fName;
        }
    }
}
