using MathNet.Numerics;
using System;

namespace SpectrumAnalyzer.Models
{
    public class Polynomial : IXYFunction
    {
        public int Order { get { return Coefficients.Length - 1; } }
        public double[] Coefficients { get; private set; } = { 0, 1, 2 };

        public Polynomial(double[] coefficients)
        {
            Coefficients = coefficients;
        }
        public Polynomial()
        {
        }

        public void FitToData(double[] xData, double[] yData, int order)
        {
            Coefficients = Fit.Polynomial(xData, yData, order);
        }

        public double GetYValue(double x)
        {
            double output = 0;

            for(int i = 0; i < Coefficients.Length; i++)
                output += Coefficients[i] * Math.Pow(x, i);

            return output;
        }
    }
}
