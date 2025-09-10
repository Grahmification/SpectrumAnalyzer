namespace SpectrumAnalyzer.Models
{
    public class XAxisUnits
    {
        public string TimeDescription { get; private set; } = "";
        public string TimeUnit { get; private set; } = "";
        public string TimeString => $"{TimeDescription} [{TimeUnit}]";

        public string FreqDescription { get; private set; } = "";
        public string FreqUnit { get; private set; } = "";
        public string FreqString => $"{FreqDescription} [{FreqUnit}]";

        public bool FreqPrimary { get; private set; } = true;

        public XAxisUnits(string timeDescription, string timeUnit, bool freqPrimary = false, string freqDescription = "", string freqUnit = "")
        {
            TimeDescription = timeDescription;
            TimeUnit = timeUnit;

            FreqDescription = freqDescription;
            FreqUnit = freqUnit;

            FreqPrimary = freqPrimary;

            if(freqDescription == "")
                FreqDescription = $"1/{TimeDescription}";

            if (freqUnit == "")
                FreqUnit = $"1/{TimeUnit}";
        }

        public static List<XAxisUnits> DefaultUnits()
        {
            var output = new List<XAxisUnits>
            {
                new XAxisUnits("Time", "s", true, "Frequency", "hz"),
                new XAxisUnits("Time", "ms", true, "Frequency", "Khz"),
                new XAxisUnits("Time", "us", true, "Frequency", "Mhz"),
                new XAxisUnits("Length", "mm"),
                new XAxisUnits("Length", "m"),
                new XAxisUnits("Length", "um"),
                new XAxisUnits("Length", "nm")
            };

            return output;
        }

    }
}
