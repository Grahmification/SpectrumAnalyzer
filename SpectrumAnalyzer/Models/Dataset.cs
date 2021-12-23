using System.Collections.Generic;
using System.IO;

namespace SpectrumAnalyzer.Models
{
    public class Dataset
    {
        //probably want to make all of these SortedSet<> instead of List<>
        public List<Datapoint> RawData { get; private set; } = new List<Datapoint>();
        public List<Datapoint> NormalizedData { get; private set; } = new List<Datapoint>();
        public Polynomial PolyFit { get; private set; } = new Polynomial();
        public NormalizationCurve PolyFitCurve { get; private set; } = new NormalizationCurve();

        public Dictionary<double, SignalComponent> FFTData { get; private set; } = new Dictionary<double, SignalComponent>();
      
        public double MinFrequency { get { return FFT.MinFrequency(ConvertData(RawData)); } }
        public double MaxFrequency { get { return FFT.MaxFrequency(ConvertData(RawData)); } }
        public double MaxPeriod { get { return 1.0/FFT.MinFrequency(ConvertData(RawData)); } }
        public double MinPeriod { get { return 1.0/FFT.MaxFrequency(ConvertData(RawData)); } }


        public Dataset(string filePath)
        {
            LoadFromCSV(filePath);
        }


        public void ComputePolyFit(int order)
        {
            var xData = new List<double>();
            var yData = new List<double>();

            for (int i = 0; i < RawData.Count; i++)
            {
                xData.Add(RawData[i].X);
                yData.Add(RawData[i].Y);
            }


            PolyFit.FitToData(xData.ToArray(), yData.ToArray(), order);

            PolyFitCurve = new NormalizationCurve();
            PolyFitCurve.Curves.Add(PolyFit);
            PolyFitCurve.ComputeCurve(xData.ToArray());

            NormalizedData = NormalizationCurve.NormalizeData(RawData, PolyFitCurve.CurvePoints);
        }

        public void ComputeFFT(List<Datapoint> data)
        {
            var FFToutput = FFT.computeFFTComponents(ConvertData(data));

            FFTData.Clear();

            foreach (SignalComponent component in FFToutput)
                FFTData.Add(component.Frequency, component);
        }

        private void LoadFromCSV(string filePath, char delimiter = ',')
        {
            using (var reader = new StreamReader(filePath))
            {          
                //check if the first line has headers or is a number
                var line = reader.ReadLine().Split(delimiter);
                 
                if(double.TryParse(line[0], out double X))
                {
                    RawData.Add(new Datapoint(double.Parse(line[0]), double.Parse(line[1])));
                }

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().Split(delimiter);
                    RawData.Add(new Datapoint(double.Parse(line[0]), double.Parse(line[1])));
                }

                var XOffset = RawData[0].X;

                for (int i = 0; i < RawData.Count; i++)
                {
                    RawData[i].X -= XOffset;
                }
                    

            }
        }


        private double[,] ConvertData(List<Datapoint> data)
        {
            var inputData = new double[data.Count, 2];

            for (int i = 0; i < data.Count; i++)
            {
                inputData[i, 0] = data[i].X;
                inputData[i, 1] = data[i].Y;
            }

            return inputData;
        }
    }
}
