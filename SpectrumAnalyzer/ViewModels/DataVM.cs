using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataVM
    {
        public ObservableCollection<Datapoint> RawData { get; private set; } = new ObservableCollection<Datapoint>();
        public ObservableCollection<Datapoint> NormalizedData { get; private set; } = new ObservableCollection<Datapoint>();
        public Polynomial PolyFit { get; private set; } = new Polynomial();
        public NormalizationCurve PolyFitCurve { get; private set; } = new NormalizationCurve();
        //public Dictionary<double, SignalComponent> FFTData { get; private set; } = new Dictionary<double, SignalComponent>();

        public DataVM(){}

        public DataVM(double[] XData, double[] YData)
        {
            for (int i = 0; i < XData.Length; i++)
            {
                RawData.Add(new Datapoint(XData[i], YData[i]));
            }

            ZeroNormalizeXValues();
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


            NormalizedData = new ObservableCollection<Datapoint>(NormalizationCurve.NormalizeData(new List<Datapoint>(RawData), PolyFitCurve.CurvePoints));
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
        private void ZeroNormalizeXValues()
        {
            var XOffset = RawData[0].X;

            for (int i = 0; i < RawData.Count; i++)
            {
                RawData[i].X -= XOffset;
            }
        }





    }
}
