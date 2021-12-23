using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumAnalyzer.Models
{
    public class NormalizationCurve
    {
        public List<IXYFunction> Curves { get; private set; } = new List<IXYFunction>();
        public List<Datapoint> CurvePoints { get; private set; } = new List<Datapoint>();

        public void ComputeCurve(double[] xValues)
        {
            for(int i = 0; i < xValues.Length; i++)
            {
                CurvePoints.Add(new Datapoint(xValues[i], ComputeCurvePoint(xValues[i], Curves.ToArray())));
            }
        }

        public void RecomputeCurve()
        {
            foreach (Datapoint point in CurvePoints)
                point.Y = ComputeCurvePoint(point.X, Curves.ToArray());
        }


        public static double[] ComputeCurvePoints(double[] xValues, IXYFunction[] Curves)
        {
            var output = new List<double>();

            for(int i = 0; i< xValues.Length; i++)
            {
                output.Add(ComputeCurvePoint(xValues[i], Curves));
            }

            return output.ToArray();
        }

        public static double ComputeCurvePoint(double xValue, IXYFunction[] Curves)
        {
            double yValue = 0;

            foreach (IXYFunction curve in Curves)
                yValue += curve.GetYValue(xValue);

            return yValue;
        }

        public static List<Datapoint> NormalizeData(List<Datapoint> rawData, List<Datapoint> normalizingCurve)
        {
            var output = new List<Datapoint>();
            
            for(int i = 0; i < rawData.Count; i++)
            {
                output.Add(new Datapoint(rawData[i].X, rawData[i].Y - normalizingCurve[i].Y));           
            }

            return output;
        }
    }
}
