using System.Collections.Generic;


namespace SpectrumAnalyzer.Models
{
    public class SignalReconstruction : IXYFunction
    {
        public List<SignalComponent> Components { get; private set; } = new List<SignalComponent>();
        public List<Datapoint> Points { get; private set; } = new List<Datapoint>();

        public string Name { get; set; } = "";

        public SignalReconstruction(List<SignalComponent> components, string name = "")
        {
            Components = components;
            Name = name;
        }
        public void PopulatePoints(List<double> XValues)
        {
            foreach(double value in XValues)
            {
                Points.Add(new Datapoint(value, GetYValue(value)));
            }
        }
        public double GetYValue(double xValue)
        {
            return SignalComponent.ComputeYValueSum(Components, xValue);
        }
    }
}
