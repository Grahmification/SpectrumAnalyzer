using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpectrumAnalyzer.Models;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer.Services
{
    /// <summary>
    /// Service to save and load the project data file
    /// </summary>
    public static class ProjectFileService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        /// <summary>
        /// Save the project data file
        /// </summary>
        /// <param name="filePath">The save filepath</param>
        /// <param name="dataPlotVM">The current program data to save</param>
        public static void Save(string filePath, DataPlotVM dataPlotVM)
        {
            var project = new ProjectFile
            {
                // ---- source / units ----
                DataFilePath = dataPlotVM.Data.DataFilePath,
                DataTitle = dataPlotVM.Units.DataTitle,
                YAxisTitle = dataPlotVM.Units.YAxisTitle
            };

            var xu = dataPlotVM.Units.SelectedXUnit;
            project.XUnitTimeDescription = xu.TimeDescription;
            project.XUnitTimeUnit        = xu.TimeUnit;
            project.XUnitFreqDescription = xu.FreqDescription;
            project.XUnitFreqUnit        = xu.FreqUnit;
            project.XUnitFreqPrimary     = xu.FreqPrimary;

            // ---- raw data ----
            project.RawData = [.. dataPlotVM.Data.RawData.Select(p => new DatapointData { X = p.X, Y = p.Y })];

            // ---- poly fit ----
            project.PolyFitEnabled = dataPlotVM.Data.PolyFit.Enabled;
            project.PolyFitOrder   = dataPlotVM.Data.PolyFit.PolyFitOrder;

            // ---- FFT components ----
            var components = dataPlotVM.FFT.SignalComponents.ToList();
            project.SignalComponents = [.. components.Select(c => new SignalComponentData
            {
                Index            = c.Index,
                Frequency        = c.Frequency,
                RealComponent    = c.RealComponent,
                ImaginaryComponent = c.ImaginaryComponent,
                DatasetSize      = dataPlotVM.Data.FFTInputData.Count
            })];

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

        /// <summary>
        /// Load the project file into the code
        /// </summary>
        /// <param name="filePath">Path to the existing project file</param>
        /// <param name="dataPlotVM">Viewmodel to populate with project data</param>
        /// <exception cref="InvalidDataException"></exception>
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

            // ---- poly fit settings ----
            dataPlotVM.Data.PolyFit.PolyFitOrder = project.PolyFitOrder;
            dataPlotVM.Data.PolyFit.Enabled = project.PolyFitEnabled;

            // ---- raw data ----
            double[] xData = [.. project.RawData.Select(p => p.X)];
            double[] yData = [.. project.RawData.Select(p => p.Y)];
            
            // Set afrer applying fit settings, as this computes the fit
            dataPlotVM.SetData(xData, yData, project.DataTitle);

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

            // Set the FFT output data
            // This will fire the FFT completed event, which populates other things
            dataPlotVM.Data.SetFFTData(signalComponents);

            // ---- reconstructions ----
            var loadedComponents = dataPlotVM.FFT.SignalComponents.ToList();
            foreach (var rd in project.Reconstructions)
            {
                var selectedComponents = rd.ComponentIndices
                    .Where(i => i >= 0 && i < loadedComponents.Count)
                    .Select(i => loadedComponents[i])
                    .ToList();

                dataPlotVM.FFT.AddReconstruction(rd.Name, rd.InterpolationFactor, selectedComponents);
            }
        }
    }
}
