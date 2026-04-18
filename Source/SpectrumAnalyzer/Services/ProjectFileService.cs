using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer.Services
{
    public static class ProjectFileService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        // -----------------------------------------------------------------------
        // Save
        // -----------------------------------------------------------------------
        public static void Save(string filePath, DataPlotVM dataPlotVM)
        {
            var project = new ProjectFile();

            // ---- source / units ----
            project.DataFilePath = dataPlotVM.Data.DataFilePath;
            project.DataTitle    = dataPlotVM.Units.DataTitle;
            project.YAxisTitle   = dataPlotVM.Units.YAxisTitle;

            var xu = dataPlotVM.Units.SelectedXUnit;
            project.XUnitTimeDescription = xu.TimeDescription;
            project.XUnitTimeUnit        = xu.TimeUnit;
            project.XUnitFreqDescription = xu.FreqDescription;
            project.XUnitFreqUnit        = xu.FreqUnit;
            project.XUnitFreqPrimary     = xu.FreqPrimary;

            // ---- raw data ----
            // We save the zero-normalised data that is actually in memory (same as
            // what would be re-imported), so round-trip is lossless.
            project.RawData = dataPlotVM.Data.RawData
                .Select(p => new DatapointData { X = p.X, Y = p.Y })
                .ToList();

            // ---- poly fit ----
            project.PolyFitEnabled = dataPlotVM.Data.PolyFit.Enabled;
            project.PolyFitOrder   = dataPlotVM.Data.PolyFit.PolyFitOrder;

            // ---- FFT components ----
            var components = dataPlotVM.FFT.SignalComponents.ToList();
            project.SignalComponents = components.Select(c => new SignalComponentData
            {
                Index            = c.Index,
                Frequency        = c.Frequency,
                RealComponent    = c.RealComponent,
                ImaginaryComponent = c.ImaginaryComponent,
                // Back-calculate dataset size from magnitude formula so we can
                // reconstruct the SignalComponent object faithfully.
                DatasetSize      = ReconstructDatasetSize(c)
            }).ToList();

            // ---- reconstructions ----
            project.Reconstructions = dataPlotVM.FFT.Reconstructions.Select(r =>
            {
                var indices = r.Function.Curves
                    .OfType<SignalComponent>()
                    .Select(sc => components.IndexOf(sc))
                    .Where(i => i >= 0)
                    .ToList();

                return new ReconstructionData
                {
                    Name               = r.Name,
                    InterpolationFactor = r.InterpolationFactor,
                    ComponentIndices   = indices
                };
            }).ToList();

            var json = JsonSerializer.Serialize(project, _jsonOptions);
            File.WriteAllText(filePath, json);
        }

        // -----------------------------------------------------------------------
        // Load  –  returns a populated DataPlotVM ready to be used by MainVM
        // -----------------------------------------------------------------------
        public static void Load(string filePath, DataPlotVM dataPlotVM)
        {
            var json    = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<ProjectFile>(json, _jsonOptions)
                          ?? throw new InvalidDataException("Could not parse project file.");

            // ---- units (must be set before SetData triggers plot updates) ----
            var matchedUnit = dataPlotVM.Units.XUnits.FirstOrDefault(u =>
                u.TimeUnit == project.XUnitTimeUnit &&
                u.TimeDescription == project.XUnitTimeDescription) ??
                new XAxisUnits(
                    project.XUnitTimeDescription,
                    project.XUnitTimeUnit,
                    project.XUnitFreqPrimary,
                    project.XUnitFreqDescription,
                    project.XUnitFreqUnit);

            dataPlotVM.Units.SelectedXUnit = matchedUnit;
            dataPlotVM.Units.DataTitle     = project.DataTitle;
            dataPlotVM.Units.YAxisTitle    = project.YAxisTitle;

            // ---- raw data ----
            double[] xData = project.RawData.Select(p => p.X).ToArray();
            double[] yData = project.RawData.Select(p => p.Y).ToArray();

            // SetData zero-normalises; our saved data is already zero-normalised,
            // so we bypass SetData and populate directly to avoid double-shifting.
            dataPlotVM.Data.LoadRawDataDirect(xData, yData, project.DataFilePath);

            // ---- poly fit settings ----
            dataPlotVM.Data.PolyFit.PolyFitOrder = project.PolyFitOrder;

            if (project.PolyFitEnabled || dataPlotVM.Data.RawData.Count > 0)
            {
                dataPlotVM.Data.ComputeFit(null);
            }
            dataPlotVM.Data.PolyFit.Enabled = project.PolyFitEnabled;

            // ---- notify plot ----
            dataPlotVM.RefreshAfterLoad();

            // ---- FFT components ----
            if (project.SignalComponents.Count == 0)
                return;

            var signalComponents = project.SignalComponents
                .Select(sc => new SignalComponent(
                    sc.Frequency,
                    sc.RealComponent,
                    sc.ImaginaryComponent,
                    sc.Index,
                    sc.DatasetSize))
                .ToList();

            // Re-compute contribution fractions & unwrap phases (same as computeFFTComponents)
            var magnitudeSum = SignalComponent.ComputeTotalMagnitude(signalComponents);
            for (int i = 0; i < signalComponents.Count; i++)
                signalComponents[i].SetContributionFraction(magnitudeSum);
            SignalComponent.UnwrapPhases(signalComponents);

            // Build the same dictionary DataVM uses
            var fftDict = signalComponents.ToDictionary(c => c.Frequency, c => c);
            dataPlotVM.Data.LoadFFTDirect(fftDict);

            // This triggers the same FFTVM population path as a normal FFT compute
            dataPlotVM.OnFFTCompletedFromLoad();

            // ---- reconstructions ----
            var loadedComponents = dataPlotVM.FFT.SignalComponents.ToList();
            foreach (var rd in project.Reconstructions)
            {
                var selectedComponents = rd.ComponentIndices
                    .Where(i => i >= 0 && i < loadedComponents.Count)
                    .Select(i => loadedComponents[i])
                    .ToList();

                dataPlotVM.FFT.AddReconstructionFromLoad(rd.Name, rd.InterpolationFactor, selectedComponents);
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Reverse-engineers the dataset size from a SignalComponent's stored
        /// magnitude so the component can be reconstructed identically.
        /// For DC offsets: datasetSize = real / magnitude.
        /// For others: datasetSize = sqrt(real²+imag²)*2 / magnitude.
        /// We store it explicitly now, but keep this for older files.
        /// </summary>
        private static int ReconstructDatasetSize(SignalComponent c)
        {
            // We now save DatasetSize directly, so this is only a fallback.
            // The SignalComponent doesn't expose DatasetSize publicly, so we
            // must recalculate.  For DC: mag = real/n  =>  n = real/mag.
            // For AC: mag = sqrt(r²+i²)*2/n  =>  n = sqrt(r²+i²)*2/mag.
            if (c.Magnitude == 0) return 1;

            if (c.DCOffset)
                return (int)Math.Round(c.RealComponent / c.Magnitude);

            double raw = Math.Sqrt(c.RealComponent * c.RealComponent +
                                   c.ImaginaryComponent * c.ImaginaryComponent);
            return (int)Math.Round(raw * 2.0 / c.Magnitude);
        }
    }
}
