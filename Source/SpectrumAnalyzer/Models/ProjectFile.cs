namespace SpectrumAnalyzer.Models
{
    /// <summary>
    /// Root object serialized to/from a .saproj JSON file.
    /// </summary>
    public class ProjectFile
    {
        public string Version { get; set; } = "1.0";

        // ---- Source info ----
        public string DataFilePath { get; set; } = "";
        public string DataTitle { get; set; } = "";

        // ---- Units ----
        public string XUnitTimeDescription { get; set; } = "Time";
        public string XUnitTimeUnit { get; set; } = "s";
        public string XUnitFreqDescription { get; set; } = "Frequency";
        public string XUnitFreqUnit { get; set; } = "hz";
        public bool XUnitFreqPrimary { get; set; } = true;
        public string YAxisTitle { get; set; } = "Y Data";

        // ---- Raw data ----
        public List<DatapointData> RawData { get; set; } = [];

        // ---- Poly fit settings ----
        public bool PolyFitEnabled { get; set; } = false;
        public int PolyFitOrder { get; set; } = 1;

        // ---- FFT signal components (computed, saved so we don't recompute) ----
        public List<SignalComponentData> SignalComponents { get; set; } = [];

        // ---- Reconstructions ----
        public List<ReconstructionData> Reconstructions { get; set; } = [];
    }

    public class DatapointData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class SignalComponentData
    {
        public int Index { get; set; }
        public double Frequency { get; set; }
        public double RealComponent { get; set; }
        public double ImaginaryComponent { get; set; }
    }

    public class ReconstructionData
    {
        public string Name { get; set; } = "";
        public int InterpolationFactor { get; set; } = 0;
        /// <summary>Indices into SignalComponents list that belong to this reconstruction.</summary>
        public List<int> ComponentIndices { get; set; } = [];
    }
}
