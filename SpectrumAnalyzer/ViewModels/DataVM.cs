using SpectrumAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace SpectrumAnalyzer.ViewModels
{
    public class DataVM : ObservableObject
    {
        public string DataFilePath { get; set; } = "";

        public SelectedItemCollection<Datapoint> SelectedData { get; set; } = new SelectedItemCollection<Datapoint>();
        public DatapointCollection RawData { get; private set; } = new DatapointCollection();
        public DatapointCollection FitCurveData { get; private set; } = new DatapointCollection();
        public DatapointCollection NormalizedData { get; private set; } = new DatapointCollection();
        public DatapointCollection FFTInputData { get { return FitEnabled ? NormalizedData : RawData; } }

        public bool FitEnabled { get { return PolyFit.Enabled; } } //will need to or multiple of these if more fit types in the future
        public CompositeXYFunction FitCurve { get; private set; } = new CompositeXYFunction();
        public PolyFitVM PolyFit { get; private set; } = new PolyFitVM();

        public double MinFrequency { get { return DataExists() ? FFT.MinFrequency(RawData.GetFFTDataFormat()) : 0; } }
        public double MaxFrequency { get { return DataExists() ? FFT.MaxFrequency(RawData.GetFFTDataFormat()) : 0; } }
        public double MaxPeriod { get { return DataExists() ? 1.0 / FFT.MinFrequency(RawData.GetFFTDataFormat()) : 0; } }
        public double MinPeriod { get { return DataExists() ? 1.0 / FFT.MaxFrequency(RawData.GetFFTDataFormat()) : 0; } }

        public Dictionary<double, SignalComponent> FFTData { get; private set; } = new Dictionary<double, SignalComponent>();

        public event EventHandler? FitCompleted;
        public event EventHandler? FFTCompleted;

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
            var dataPoints = new List<Datapoint>();
            
            for (int i = 0; i < XData.Length; i++)
            {
                dataPoints.Add(new Datapoint(XData[i], YData[i]));
            }

            // Set data at once to avoid raising a bunch of CollectionChanged events
            RawData.SetData(dataPoints);
            RawData.ZeroNormalizeXValues();

            OnPropertyChanged("MinFrequency");
            OnPropertyChanged("MaxFrequency");
            OnPropertyChanged("MinPeriod");
            OnPropertyChanged("MaxPeriod");
        }
        public void ComputeFit(object? parameter)
        {
            PolyFit.FitToData(RawData.XValues.ToArray(), RawData.YValues.ToArray());

            FitCurve = new CompositeXYFunction();
            FitCurve.Curves.Add(PolyFit.PolyFunction);
            
            FitCurveData.SetData(FitCurve.ComputeFunction(RawData));
            NormalizedData.SetData(DatapointCollection.YValueMultisetOperation(RawData, FitCurveData, (a, b) => a - b));

            FitCompleted?.Invoke(this, new EventArgs());
        }
        public void ComputeFFT(object? parameter)
        {
            var FFToutput = FFT.computeFFTComponents(FFTInputData.GetFFTDataFormat());

            FFTData.Clear();

            foreach (SignalComponent component in FFToutput)
                FFTData.Add(component.Frequency, component);

            FFTCompleted?.Invoke(this, new EventArgs());
        }
        public bool DataExists()
        {
            return RawData.Count > 0;
        }
    }
}
