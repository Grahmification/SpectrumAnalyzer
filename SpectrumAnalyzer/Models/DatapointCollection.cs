using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpectrumAnalyzer.Models
{
    public class DatapointCollection: ObservableCollection<Datapoint>
    {
        public IList<double> XValues { get { return Items.Select(c => c.X).ToList(); } }
        public IList<double> YValues { get { return Items.Select(c => c.Y).ToList(); } }

        public void XValueOperation(Func<double, double> operation)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].X = operation(Items[i].X);
            }
        }
        public void YValueOperation(Func<double, double> operation)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Y = operation(Items[i].Y);
            }
        }
        public void ZeroNormalizeXValues()
        {
            XValueOperation(x => x - Items[0].X);         
        }


        public double[,] GetFFTDataFormat()
        {
            var inputData = new double[Items.Count, 2];

            for (int i = 0; i < Items.Count; i++)
            {
                inputData[i, 0] = Items[i].X;
                inputData[i, 1] = Items[i].Y;
            }

            return inputData;
        }

        public static IList<Datapoint> YValueMultisetOperation(IList<Datapoint> dataset1, IList<Datapoint> dataset2, Func<double, double, double> operation)
        {
            var output = new List<Datapoint>();

            for (int i = 0; i < dataset1.Count; i++)
            {
                output.Add(new Datapoint(dataset1[i].X, operation(dataset1[i].Y, dataset2[i].Y)));
            }

            return output;
        }

    }
}
