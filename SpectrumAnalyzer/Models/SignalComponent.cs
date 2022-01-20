using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumAnalyzer.Models
{
    public class SignalComponent : IXYFunction
    {
        public bool DCOffset { get; private set; } = false;
        public int Index { get; private set; } = 0;
        public double RealComponent { get; private set; } = 0;
        public double ImaginaryComponent { get; private set; } = 0;

        public double Frequency { get; private set; } = 0;
        public double Period
        {
            get
            {
                if (Frequency == 0)
                    return 0;
                else
                    return 1 / Frequency;
            }
        }

        public double ContributionFraction { get; private set; } = 0;
        public double Magnitude { get; private set; } = 0;
        public double Phase { get; private set; } = 0;

        public SignalComponent(double freqency, double real, double imaginary, int index, int datasetSize)
        {
            Frequency = freqency;
            RealComponent = real;
            ImaginaryComponent = imaginary;
            Index = index;

            //if the imaginary component is 0 this signal is the DC offset
            if (imaginary == 0)
            {
                DCOffset = true;
                Frequency = 0;
                Magnitude = real / datasetSize;
                Phase = 0;
            }
            else
            {
                DCOffset = false;
                Magnitude = Math.Sqrt((real * real) + (imaginary * imaginary)) * 2 / datasetSize;
                Phase = Math.Atan2(imaginary, real);
            }
        }
        public SignalComponent() { }

        public double GetYValue(double xValue)
        {
            if (DCOffset)
            {
                return Magnitude;
            }
            else
            {
                return Magnitude * Math.Cos(2 * Math.PI * Frequency * xValue + Phase);
            }

        }
        public void SetContributionFraction(double totalMagnitude)
        {
            ContributionFraction = Magnitude / totalMagnitude;
        }
        public void UnwrapPhase(double previousPhase)
        {
            while(Phase - previousPhase > Math.PI)
            {
                Phase -= 2 * Math.PI;
            }

            while (previousPhase - Phase > Math.PI)
            {
                Phase += 2 * Math.PI;
            }
        }

        public static void UnwrapPhases(IList<SignalComponent> components)
        {
            for(int i = 1; i< components.Count; i++)
            {
                components[i].UnwrapPhase(components[i - 1].Phase);
            }
        }
        public static double ComputeTotalMagnitude(IList<SignalComponent> components)
        {
            return components.Sum(s => s.Magnitude);
        }
        public static  string[] GetExportHeader(string freqUnits = "hz", string timeUnits = "s", string magnitudeUnits = "-")
        {
            return new string[]
            {
                "Index",
                string.Format("Frequency [{0}]", freqUnits),
                string.Format("Period [{0}]", timeUnits),
                string.Format("Magnitude [{0}]", magnitudeUnits),
                "Phase",
                "Contribution Fraction"
            };
        }
        public string[] GetExportDataLine()
        {
            return new string[] { Index.ToString(), Frequency.ToString(), Period.ToString(), Magnitude.ToString(), Phase.ToString(), ContributionFraction.ToString() };
        }
    }
}
