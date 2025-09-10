using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class SignalReconstructionVM : ObservableObject
    {
        public CompositeXYFunction Function { get; private set; } = new CompositeXYFunction();
        public DatapointCollection Points { get; private set; } = new DatapointCollection();
        public DatapointCollection NonInterpolatedPoints { get; private set; } = new DatapointCollection();

        private int _interpolationFactor = 0;
        public int InterpolationFactor 
        { 
            get { return _interpolationFactor; }
            set { if (value > 0) { _interpolationFactor = value; } else { _interpolationFactor = 0; } PopulateInterpolatedPoints(); }
        }

        public string Name { get; set; } = "";

        public SignalReconstructionVM() { }
        public SignalReconstructionVM(string name = "")
        {
            Name = name;
        }
        public void PopulateComponents(IList<SignalComponent> components)
        {
            Function.Curves.Clear();
            Points.Clear();
            NonInterpolatedPoints.Clear();
            
            foreach (SignalComponent comp in components)
                Function.Curves.Add(comp);
        }
        public void PopulatePoints(List<double> XValues, int interpolationFactor = -1)
        {
            if (interpolationFactor != -1)
                _interpolationFactor = interpolationFactor;
            
            NonInterpolatedPoints.SetData(Function.ComputeFunction(XValues.ToArray()));
            PopulateInterpolatedPoints();
        }

        private void PopulateInterpolatedPoints()
        {
            List<double> values = new List<double>(DatapointCollection.InterpolateList(NonInterpolatedPoints.XValues, _interpolationFactor));
            Points.SetData(Function.ComputeFunction(values.ToArray()));
        }
    }
}
