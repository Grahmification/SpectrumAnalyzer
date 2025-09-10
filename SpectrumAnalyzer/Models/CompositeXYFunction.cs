namespace SpectrumAnalyzer.Models
{
    public class CompositeXYFunction : IXYFunction
    {
        public List<IXYFunction> Curves { get; private set; } = new List<IXYFunction>();

        public CompositeXYFunction()
        {
            Curves = [];
        }
        public double GetYValue(double xValue)
        { 
            return Curves.Sum(s => s.GetYValue(xValue));
        }
        public ICollection<Datapoint> ComputeFunction(double[] xValues)
        {
            var output = new List<Datapoint>();
            
            for (int i = 0; i < xValues.Length; i++)
            {
                output.Add(new Datapoint(xValues[i], GetYValue(xValues[i])));
            }

            return output;
        }
        public ICollection<Datapoint> ComputeFunction(ICollection<Datapoint> function)
        {
            var output = new List<Datapoint>();

            foreach(Datapoint p in function)
            {
                output.Add(new Datapoint(p.X, GetYValue(p.X)));
            }

            return output;
        }
    }
}
