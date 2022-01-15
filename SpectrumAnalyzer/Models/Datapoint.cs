using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectrumAnalyzer.Models
{
    public class Datapoint : IComparable<Datapoint>, IDataPointProvider
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Datapoint(double x)
        {
            X = x;
            Y = 0;
        }

        public Datapoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public int CompareTo(Datapoint other)
        {
            return X.CompareTo(other.X);
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(X, Y);
        }

        public static IList<double> XValues(IList<Datapoint> dataset)
        {
            return dataset.Select(c => c.X).ToList();
        }

        public static IList<double> YValues(IList<Datapoint> dataset)
        {
            return dataset.Select(c => c.Y).ToList();
        }

    }
}
