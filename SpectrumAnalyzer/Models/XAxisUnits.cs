namespace SpectrumAnalyzer.Models
{
    public class XAxisUnits
    {
        public string TimeDescription { get; private set; } = "";
        public string TimeUnit { get; private set; } = "";
        public string TimeString { get { return string.Format("{0} [{1}]", TimeDescription, TimeUnit); } }

        public string FreqDescription { get; private set; } = "";
        public string FreqUnit { get; private set; } = "";
        public string FreqString { get { return string.Format("{0} [{1}]", FreqDescription, FreqUnit); } }

        public bool FreqPrimary { get; private set; } = true;

        public XAxisUnits(string timeDescription, string timeUnit, bool freqPrimary = false, string freqDescription = "", string freqUnit = "")
        {
            TimeDescription = timeDescription;
            TimeUnit = timeUnit;

            FreqDescription = freqDescription;
            FreqUnit = freqUnit;

            FreqPrimary = freqPrimary;

            if(freqDescription == "")
                FreqDescription = string.Format("1/{0}", TimeDescription);

            if (freqUnit == "")
                FreqUnit = string.Format("1/{0}", TimeUnit);
        }

        public static List<XAxisUnits> DefaultUnits()
        {
            var output = new List<XAxisUnits>();

            output.Add(new XAxisUnits("Time", "s", true, "Frequency", "hz"));
            output.Add(new XAxisUnits("Time", "ms", true, "Frequency", "Khz"));
            output.Add(new XAxisUnits("Time", "us", true, "Frequency", "Mhz"));
            output.Add(new XAxisUnits("Length", "mm"));
            output.Add(new XAxisUnits("Length", "m"));
            output.Add(new XAxisUnits("Length", "um"));
            output.Add(new XAxisUnits("Length", "nm"));

            return output;
        }

    }
}
