using System.Collections.ObjectModel;

namespace SpectrumAnalyzer.Models
{
    public class DatapointCollection: ObservableCollection<Datapoint>
    {
        public IList<double> XValues { get { return Items.Select(c => c.X).ToList(); } }
        public IList<double> YValues { get { return Items.Select(c => c.Y).ToList(); } }

        public DatapointCollection() { }
        public void SetData(ICollection<Datapoint> dataset)
        {
            Items.Clear();
            
            foreach (Datapoint p in dataset)
                Items.Add(p);
        }
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
            double XOffset = Items[0].X;
            XValueOperation(x => x - XOffset);         
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

        public static double InterpolateValue(double y0, double y1, double x, double x0 = 0, double x1 = 1)
        {
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }
        public static IList<double> InterpolateList(IList<double> inputVals, int interpolationFactor)
        {
            if (interpolationFactor < 0)
                interpolationFactor = 0;
            
            List<double> output = [];

            for(int i = 0; i<inputVals.Count-1; i++)
            {
                output.Add(inputVals[i]);
                
                for (int j = 0; j < interpolationFactor; j++)
                {
                    double x = (j + 1.0) / (interpolationFactor + 1.0);
                    output.Add(InterpolateValue(inputVals[i], inputVals[i + 1], x));
                }
            }

            output.Add(inputVals.LastOrDefault());

            return output;
        }

    }
}
