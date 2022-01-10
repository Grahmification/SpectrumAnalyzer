using SpectrumAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataVM : ObservableObject
    {
        public DatapointCollection RawData { get; private set; } = new DatapointCollection();
        public DatapointCollection FitCurveData { get; private set; } = new DatapointCollection();
        public DatapointCollection NormalizedData { get; private set; } = new DatapointCollection();
        public DatapointCollection FFTInputData { get { return EnableFit ? NormalizedData : RawData; } }

        public CompositeXYFunction FitCurve { get; private set; } = new CompositeXYFunction();
        public bool EnableFit { get; set; } = true;

        public PolyFitVM PolyFit { get; private set; } = new PolyFitVM();

        

        public double MinFrequency { get { return FFT.MinFrequency(RawData.GetFFTDataFormat()); } }
        public double MaxFrequency { get { return FFT.MaxFrequency(RawData.GetFFTDataFormat()); } }
        public double MaxPeriod { get { return 1.0 / FFT.MinFrequency(RawData.GetFFTDataFormat()); } }
        public double MinPeriod { get { return 1.0 / FFT.MaxFrequency(RawData.GetFFTDataFormat()); } }


        public Dictionary<double, SignalComponent> FFTData { get; private set; } = new Dictionary<double, SignalComponent>();

        /// <summary>
        /// RelayCommand for <see cref="ComputeFFT"/>
        /// </summary>
        public ICommand ComputeFFTCommand { get; private set; }

        /// <summary>
        /// RelayCommand for <see cref="ComputeFit"/>
        /// </summary>
        public ICommand ComputeFitCommand { get; private set; }


        public DataVM()
        {
            ComputeFFTCommand = new RelayCommand<object>(ComputeFFT, DataExists);
            ComputeFitCommand = new RelayCommand<object>(ComputeFit, DataExists);
        }

        public void SetData(double[] XData, double[] YData)
        {
            RawData.Clear();
            
            for (int i = 0; i < XData.Length; i++)
            {
                RawData.Add(new Datapoint(XData[i], YData[i]));
            }

            RawData.ZeroNormalizeXValues();
        }
        public void ComputeFit(object parameter)
        {
            PolyFit.FitToData(RawData.XValues.ToArray(), RawData.YValues.ToArray());

            FitCurve = new CompositeXYFunction();
            FitCurve.Curves.Add(PolyFit.PolyFunction);
            
            FitCurveData = (DatapointCollection)FitCurve.ComputeFunction(RawData);
            NormalizedData = (DatapointCollection)DatapointCollection.YValueMultisetOperation(RawData, FitCurveData, (a, b) => a - b);
        }
        public void ComputeFFT(object parameter)
        {
            var FFToutput = FFT.computeFFTComponents(FFTInputData.GetFFTDataFormat());

            FFTData.Clear();

            foreach (SignalComponent component in FFToutput)
                FFTData.Add(component.Frequency, component);
        }
        public bool DataExists()
        {
            return RawData.Count > 0;
        }

    }
}
