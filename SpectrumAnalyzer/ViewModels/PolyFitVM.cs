using SpectrumAnalyzer.Models;

namespace SpectrumAnalyzer.ViewModels
{
    public class PolyFitVM : ObservableObject
    {
        public Polynomial PolyFunction { get; private set; } = new Polynomial();
        public int PolyFitOrder { get; set; } = 1;
        public double[] PolyCoefs { get; private set; } = [];
        public string PolyCoefsString => Enabled? string.Join(",", Array.ConvertAll(PolyCoefs, s => s.ToString())) : "";

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; FitEnableChanged?.Invoke(this, _enabled); }
        }
        private bool _enabled = false;

        public event EventHandler<bool>? FitEnableChanged;

        public void FitToData(double[] xData, double[] yData)
        {
            PolyFunction.FitToData(xData, yData, PolyFitOrder);
            PolyCoefs = PolyFunction.Coefficients;
        }
    }
}
