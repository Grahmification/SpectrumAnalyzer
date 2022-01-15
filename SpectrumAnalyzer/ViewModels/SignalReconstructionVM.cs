using SpectrumAnalyzer.Models;
using System.Collections.Generic;

namespace SpectrumAnalyzer.ViewModels
{
    public class SignalReconstructionVM : ObservableObject
    {
        public CompositeXYFunction Function { get; private set; } = new CompositeXYFunction();
        public DatapointCollection Points { get; private set; } = new DatapointCollection();
        public string Name { get; set; } = "";

        public SignalReconstructionVM() { }
        public SignalReconstructionVM(IList<SignalComponent> components, string name = "")
        {
            Name = name;

            foreach (SignalComponent comp in components)
                Function.Curves.Add(comp);
        }
        public void PopulatePoints(List<double> XValues)
        {
            Points.SetData(Function.ComputeFunction(XValues.ToArray()));
        }
    }
}
