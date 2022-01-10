using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumAnalyzer.ViewModels
{
    public class PolyFitVM : ObservableObject
    {
        public Polynomial PolyFunction { get; private set; } = new Polynomial();
        public int PolyFitOrder { get; set; } = 3;
        public double[] PolyCoefs { get; private set; } = new double[] { };

        public void FitToData(double[] xData, double[] yData)
        {
            PolyFunction.FitToData(xData, yData, PolyFitOrder);
            PolyCoefs = PolyFunction.Coefficients;
        }
    }
}
