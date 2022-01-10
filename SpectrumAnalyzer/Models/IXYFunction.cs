using System.Collections.Generic;

namespace SpectrumAnalyzer.Models
{
    public interface IXYFunction
    {
        double GetYValue(double xValue);

        //List<Datapoint> ComputeFunction(double[] xValues);

        //List<Datapoint> RecomputeFunction(List<Datapoint> function);
    }
}
